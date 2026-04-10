using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using CommunityToolkit.Diagnostics;
using ConfigCompare.AzureIdentity.Resources;
using Microsoft.Extensions.Logging;

namespace ConfigCompare.AzureIdentity;

/// <summary>
/// Implementation of IAuthService using Azure Identity SDK and Microsoft Graph.
/// </summary>
public class AuthService : IAuthService
{
    private readonly TokenCredential _credential;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(TokenCredential credential, HttpClient httpClient, ILogger<AuthService> logger)
    {
        Guard.IsNotNull(credential);
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(logger);
        _credential = credential;
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Acquires an access token for the specified scopes.
    /// </summary>
    public async Task<GetTokenResponse> GetTokenAsync(string[] scopes, CancellationToken cancellationToken = default)
    {
        try
        {
            if (scopes == null || scopes.Length == 0)
            {
                const string errorMessage = "Scopes must not be null or empty.";
                _logger.LogWarning(errorMessage);
                return new GetTokenResponse(false, errorMessage, null);
            }

            var tokenContext = new TokenRequestContext(scopes);
            var token = await _credential.GetTokenAsync(tokenContext, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Successfully acquired token for scopes: {Scopes}", string.Join(", ", scopes));

            var tokenInfo = new TokenInfo(token.Token, token.ExpiresOn, scopes);
            return new GetTokenResponse(true, null, tokenInfo);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Token acquisition was cancelled.");
            return new GetTokenResponse(false, "Token acquisition was cancelled.", null);
        }
        catch (AuthenticationFailedException ex)
        {
            _logger.LogError(ex, "Authentication failed.");
            return new GetTokenResponse(false, $"Authentication failed: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token acquisition.");
            return new GetTokenResponse(false, $"Unexpected error: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Retrieves the currently authenticated user's identity information from Microsoft Graph.
    /// </summary>
    public async Task<GetCurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // First, acquire a token for Microsoft Graph
            var tokenResponse = await GetTokenAsync(new[] { "https://graph.microsoft.com/.default" }, cancellationToken)
                .ConfigureAwait(false);

            if (!tokenResponse.IsSuccess || tokenResponse.Data == null)
            {
                _logger.LogWarning("Failed to obtain token for Microsoft Graph.");
                var errorMsg = tokenResponse.ErrorMessage ?? "Failed to obtain authentication token.";
                return new GetCurrentUserResponse(false, errorMsg, null);
            }

            // Call the Microsoft Graph /me endpoint
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.Data.AccessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            var displayName = root.GetProperty("displayName").GetString() ?? "(Unknown)";
            var userPrincipalName = root.GetProperty("userPrincipalName").GetString() ?? "(Unknown)";
            var objectId = root.GetProperty("id").GetString() ?? "(Unknown)";

            // Extract tenant ID from mail domain or use a default placeholder
            // In a real scenario, this would come from the token claims
            var tenantId = ExtractTenantId(userPrincipalName) ?? "unknown";

            var userInfo = new UserInfo(displayName, userPrincipalName, tenantId, objectId);
            _logger.LogInformation("Successfully retrieved user info: {UserPrincipalName}", userPrincipalName);

            return new GetCurrentUserResponse(true, null, userInfo);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while retrieving user information.");
            return new GetCurrentUserResponse(false, $"Failed to retrieve user information: {ex.Message}", null);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "User info retrieval was cancelled.");
            return new GetCurrentUserResponse(false, "Operation was cancelled.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving user information.");
            return new GetCurrentUserResponse(false, $"Unexpected error: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Checks if the current user is authenticated with Azure.
    /// </summary>
    public async Task<IsAuthenticatedResponse> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenResponse = await GetTokenAsync(new[] { "https://management.azure.com/.default" }, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Authentication check completed. IsAuthenticated: {IsAuthenticated}", tokenResponse.IsSuccess);
            return new IsAuthenticatedResponse(tokenResponse.IsSuccess, tokenResponse.ErrorMessage, tokenResponse.IsSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status.");
            return new IsAuthenticatedResponse(false, $"Error checking authentication: {ex.Message}", false);
        }
    }

    /// <summary>
    /// Extracts a tenant identifier from the UPN (simplified; in production, use token claims).
    /// </summary>
    private static string? ExtractTenantId(string? userPrincipalName)
    {
        if (string.IsNullOrWhiteSpace(userPrincipalName) || !userPrincipalName.Contains("@"))
            return null;

        var domain = userPrincipalName.Split("@").LastOrDefault();
        return domain;
    }
}
