using ConfigCompare.ResourceGroup;
using ConfigCompare.ResourceGroup.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConfigCompare.ResourceGroup.Tests.Unit;

public class ResourceGroupServiceTests
{
    private readonly Mock<Azure.ResourceManager.ArmClient> _mockArmClient = new();
    private readonly Mock<ILogger<ResourceGroupService>> _mockLogger = new();

    private ResourceGroupService CreateService()
    {
        return new ResourceGroupService(_mockArmClient.Object, _mockLogger.Object);
    }

    #region GetServicesInGroupAsync Input Validation Tests

    [Fact]
    public async Task GetServicesInGroupAsync_WithNullResourceGroupName_ReturnsFailureResponse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.GetServicesInGroupAsync(null!, CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("must not be null or empty");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetServicesInGroupAsync_WithEmptyResourceGroupName_ReturnsFailureResponse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.GetServicesInGroupAsync("", CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("must not be null or empty");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetServicesInGroupAsync_WithWhitespaceResourceGroupName_ReturnsFailureResponse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.GetServicesInGroupAsync("   ", CancellationToken.None);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("must not be null or empty");
        response.Data.Should().BeNull();
    }

    #endregion
}



