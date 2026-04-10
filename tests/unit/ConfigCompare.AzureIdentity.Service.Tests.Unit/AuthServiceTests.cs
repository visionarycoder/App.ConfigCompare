using System.Net;
using System.Net.Http;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using ConfigCompare.AzureIdentity.Contract;
using ConfigCompare.AzureIdentity.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace ConfigCompare.AzureIdentity.Service.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<TokenCredential> _mockCredential = new();
    private readonly Mock<HttpMessageHandler> _mockHttpHandler = new();
    private readonly Mock<ILogger<AuthService>> _mockLogger = new();
    private readonly HttpClient _httpClient;

    public AuthServiceTests()
    {
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://graph.microsoft.com")
        };
    }

    private AuthService CreateService()
    {
        return new AuthService(_mockCredential.Object, _httpClient, _mockLogger.Object);
    }

    #region GetTokenAsync Tests

    [Fact]
    public async Task GetTokenAsync_WithValidScopes_ReturnsSuccessResponse()
    {
        // Arrange
        var scopes = new[] { "https://management.azure.com/.default" };
        var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var accessToken = new AccessToken("test-token", expiresOn);

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(scopes, CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().Be("test-token");
        response.Data.ExpiresOn.Should().Be(expiresOn);
        response.Data.Scopes.Should().BeEquivalentTo(scopes);
    }

    [Fact]
    public async Task GetTokenAsync_WithNullScopes_ReturnsFailureResponse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(null!, CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("Scopes must not be null or empty");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WithEmptyScopes_ReturnsFailureResponse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(Array.Empty<string>(), CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("Scopes must not be null or empty");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WhenCredentialThrowsAuthenticationFailedException_ReturnsFailureResponse()
    {
        // Arrange
        var scopes = new[] { "https://management.azure.com/.default" };
        var exception = new AuthenticationFailedException("Authentication failed");

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(scopes, CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("Authentication failed");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WhenCancelled_ReturnsFailureResponse()
    {
        // Arrange
        var scopes = new[] { "https://management.azure.com/.default" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(scopes, cts.Token);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("cancelled");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTokenAsync_WhenUnexpectedExceptionThrown_ReturnsFailureResponse()
    {
        // Arrange
        var scopes = new[] { "https://management.azure.com/.default" };

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var service = CreateService();

        // Act
        var response = await service.GetTokenAsync(scopes, CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("Unexpected error");
        response.Data.Should().BeNull();
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_WithValidToken_ReturnsSuccessResponse()
    {
        // Arrange
        var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var accessToken = new AccessToken("test-token", expiresOn);

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var graphResponse = new
        {
            id = "00000000-0000-0000-0000-000000000001",
            displayName = "Test User",
            userPrincipalName = "testuser@contoso.com"
        };

        var graphJson = JsonSerializer.Serialize(graphResponse);
        var graphHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(graphJson, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(graphHttpResponse);

        var service = CreateService();

        // Act
        var response = await service.GetCurrentUserAsync(CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();
        response.Data.Should().NotBeNull();
        response.Data!.DisplayName.Should().Be("Test User");
        response.Data.UserPrincipalName.Should().Be("testuser@contoso.com");
        response.Data.ObjectId.Should().Be("00000000-0000-0000-0000-000000000001");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenTokenAcquisitionFails_ReturnsFailureResponse()
    {
        // Arrange
        var exception = new AuthenticationFailedException("Failed to get token");

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var service = CreateService();

        // Act
        var response = await service.GetCurrentUserAsync(CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().NotBeNullOrEmpty();
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenGraphApiReturnsUnsuccessful_ReturnsFailureResponse()
    {
        // Arrange
        var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var accessToken = new AccessToken("test-token", expiresOn);

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var graphHttpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(graphHttpResponse);

        var service = CreateService();

        // Act
        var response = await service.GetCurrentUserAsync(CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("Failed to retrieve user information");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenCancelled_ReturnsFailureResponse()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = CreateService();

        // Act
        var response = await service.GetCurrentUserAsync(cts.Token);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("cancelled");
        response.Data.Should().BeNull();
    }

    #endregion

    #region IsAuthenticatedAsync Tests

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenAvailable_ReturnsAuthenticatedTrue()
    {
        // Arrange
        var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var accessToken = new AccessToken("test-token", expiresOn);

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var service = CreateService();

        // Act
        var response = await service.IsAuthenticatedAsync(CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();
        response.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenUnavailable_ReturnsAuthenticatedFalse()
    {
        // Arrange
        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationFailedException("No token available"));

        var service = CreateService();

        // Act
        var response = await service.IsAuthenticatedAsync(CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.IsAuthenticated.Should().BeFalse();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetTokenAsync_LogsSuccessfulTokenAcquisition()
    {
        // Arrange
        var scopes = new[] { "https://management.azure.com/.default" };
        var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
        var accessToken = new AccessToken("test-token", expiresOn);

        _mockCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var service = CreateService();

        // Act
        await service.GetTokenAsync(scopes, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully acquired token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
