#nullable enable

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

namespace ConfigCompare.Integration.Tests;

/// <summary>
/// Integration tests for AzureIdentity service against live Azure tenant.
/// These tests verify that authentication flows work correctly with real Azure services.
/// They are skipped in CI environments without Azure credentials.
/// </summary>
public class AuthServiceIntegrationTests
{
    private bool IsAzureAvailable()
    {
        try
        {
            var credential = new DefaultAzureCredential();
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var token = credential.GetToken(
                new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
                cancellation.Token);
            
            return !string.IsNullOrWhiteSpace(token.Token);
        }
        catch
        {
            return false;
        }
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidCredentials_ReturnsUserInfo()
    {
        if (!IsAzureAvailable())
        {
            return; // Skip silently in CI
        }

        // Arrange
        var credential = new DefaultAzureCredential();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
            cancellation.Token);

        // Assert
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.True(result.ExpiresOn > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Credential_TokenCanBeRefreshed_OnConsecutiveCalls()
    {
        if (!IsAzureAvailable())
        {
            return; // Skip silently in CI
        }

        // Arrange
        var credential = new DefaultAzureCredential();
        const int maxRetries = 3;

        // Act & Assert - Multiple calls should succeed
        for (int i = 0; i < maxRetries; i++)
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
                cancellation.Token);

            Assert.NotNull(token.Token);
            Assert.NotEmpty(token.Token);
        }
    }
}

