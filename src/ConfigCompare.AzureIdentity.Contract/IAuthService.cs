namespace ConfigCompare.AzureIdentity.Contract;

/// <summary>
/// Provides Azure authentication and current user identity services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Acquires an access token for the specified scopes.
    /// </summary>
    /// <param name="scopes">Azure scope(s) for which to request the token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token response with success status and token info or error details.</returns>
    Task<GetTokenResponse> GetTokenAsync(string[] scopes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the currently authenticated user's identity information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User info response with success status and user details or error information.</returns>
    Task<GetCurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user is authenticated with Azure.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication status response.</returns>
    Task<IsAuthenticatedResponse> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
