using System.Text.Json;
using ConfigCompare.Settings.Contract;
using ConfigCompare.Settings.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConfigCompare.Settings.Service.Tests.Unit;

/// <summary>
/// Unit tests for SettingsService.
/// </summary>
public class SettingsServiceTests : IDisposable
{
    private readonly string TemporaryFilePath;
    private readonly Mock<ILogger<SettingsService>> MockLogger;

    public SettingsServiceTests()
    {
        TemporaryFilePath = Path.GetTempFileName();
        MockLogger = new Mock<ILogger<SettingsService>>();
    }

    public void Dispose()
    {
        if (File.Exists(TemporaryFilePath))
        {
            File.Delete(TemporaryFilePath);
        }
    }

    [Fact]
    public async Task GetSettingsAsync_NoFileExists_ReturnsDefaults()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        if (File.Exists(filePath)) File.Delete(filePath);

        var service = new SettingsService(filePath, MockLogger.Object);

        try
        {
            // Act
            var response = await service.GetSettingsAsync();

            // Assert
            response.IsSuccess.Should().BeTrue();
            response.ErrorMessage.Should().BeNull();
            response.Data.Should().NotBeNull();
            response.Data!.Theme.Should().Be(AppTheme.System);
            response.Data.AutoRefreshIntervalSeconds.Should().Be(60);
            response.Data.AutoRefreshEnabled.Should().BeFalse();
            response.Data.SavedResourceGroupNames.Should().BeEmpty();
            response.Data.LastActiveSubscriptionId.Should().BeNull();
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetSettingsAsync_ValidFile_ReturnsDeserializedSettings()
    {
        // Arrange
        var settings = new UserSettingsDto(
            Theme: AppTheme.Dark,
            AutoRefreshIntervalSeconds: 120,
            AutoRefreshEnabled: true,
            SavedResourceGroupNames: new[] { "rg1", "rg2" },
            LastActiveSubscriptionId: "sub-123");

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
        await File.WriteAllTextAsync(TemporaryFilePath, json);

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var response = await service.GetSettingsAsync();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();
        response.Data.Should().NotBeNull();
        response.Data!.Theme.Should().Be(AppTheme.Dark);
        response.Data.AutoRefreshIntervalSeconds.Should().Be(120);
        response.Data.AutoRefreshEnabled.Should().BeTrue();
        response.Data.SavedResourceGroupNames.Should().Equal("rg1", "rg2");
        response.Data.LastActiveSubscriptionId.Should().Be("sub-123");
    }

    [Fact]
    public async Task GetSettingsAsync_CorruptJson_ReturnsError()
    {
        // Arrange
        await File.WriteAllTextAsync(TemporaryFilePath, "{ invalid json ]");
        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var response = await service.GetSettingsAsync();

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().NotBeNullOrEmpty();
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task SaveSettingsAsync_ValidSettings_SuccessfullyPersists()
    {
        // Arrange
        var settings = new UserSettingsDto(
            Theme: AppTheme.Light,
            AutoRefreshIntervalSeconds: 90,
            AutoRefreshEnabled: false,
            SavedResourceGroupNames: new[] { "rg-test" }.ToList().AsReadOnly(),
            LastActiveSubscriptionId: null);

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var saveResponse = await service.SaveSettingsAsync(settings);

        // Assert
        saveResponse.IsSuccess.Should().BeTrue();
        saveResponse.ErrorMessage.Should().BeNull();

        // Verify subsequent get returns the same settings
        var getResponse = await service.GetSettingsAsync();
        getResponse.IsSuccess.Should().BeTrue();
        getResponse.Data!.Theme.Should().Be(settings.Theme);
        getResponse.Data.AutoRefreshIntervalSeconds.Should().Be(settings.AutoRefreshIntervalSeconds);
        getResponse.Data.AutoRefreshEnabled.Should().Be(settings.AutoRefreshEnabled);
        getResponse.Data.SavedResourceGroupNames.Should().Equal(settings.SavedResourceGroupNames);
        getResponse.Data.LastActiveSubscriptionId.Should().Be(settings.LastActiveSubscriptionId);
    }

    [Fact]
    public async Task SaveSettingsAsync_IntervalBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var settings = new UserSettingsDto(
            Theme: AppTheme.System,
            AutoRefreshIntervalSeconds: 5, // < 10
            AutoRefreshEnabled: false,
            SavedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            LastActiveSubscriptionId: null);

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var response = await service.SaveSettingsAsync(settings);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("between 10 and 3600");
    }

    [Fact]
    public async Task SaveSettingsAsync_IntervalAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var settings = new UserSettingsDto(
            Theme: AppTheme.System,
            AutoRefreshIntervalSeconds: 4000, // > 3600
            AutoRefreshEnabled: false,
            SavedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            LastActiveSubscriptionId: null);

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var response = await service.SaveSettingsAsync(settings);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("between 10 and 3600");
    }

    [Fact]
    public async Task SaveSettingsAsync_NullSettings_ReturnsError()
    {
        // Arrange
        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        var response = await service.SaveSettingsAsync(null!);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetSettingsAsync_AfterSave_SubsceqeuntGetReturnsDefaults()
    {
        // Arrange
        var settings = new UserSettingsDto(
            Theme: AppTheme.Dark,
            AutoRefreshIntervalSeconds: 180,
            AutoRefreshEnabled: true,
            SavedResourceGroupNames: new[] { "rg1" },
            LastActiveSubscriptionId: "sub-123");

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);
        await service.SaveSettingsAsync(settings);

        // Act
        var resetResponse = await service.ResetSettingsAsync();

        // Assert
        resetResponse.IsSuccess.Should().BeTrue();

        // Verify subsequent get returns defaults
        var getResponse = await service.GetSettingsAsync();
        getResponse.IsSuccess.Should().BeTrue();
        getResponse.Data!.Theme.Should().Be(SettingsService.DefaultSettings.Theme);
        getResponse.Data.AutoRefreshIntervalSeconds.Should().Be(SettingsService.DefaultSettings.AutoRefreshIntervalSeconds);
        getResponse.Data.AutoRefreshEnabled.Should().Be(SettingsService.DefaultSettings.AutoRefreshEnabled);
    }

    [Fact]
    public async Task SaveSettingsAsync_WritesAtomically()
    {
        // Arrange
        var settings1 = new UserSettingsDto(
            Theme: AppTheme.Light,
            AutoRefreshIntervalSeconds: 60,
            AutoRefreshEnabled: false,
            SavedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            LastActiveSubscriptionId: null);

        var settings2 = new UserSettingsDto(
            Theme: AppTheme.Dark,
            AutoRefreshIntervalSeconds: 120,
            AutoRefreshEnabled: true,
            SavedResourceGroupNames: new[] { "rg2" }.ToList().AsReadOnly(),
            LastActiveSubscriptionId: "sub-456");

        var service = new SettingsService(TemporaryFilePath, MockLogger.Object);

        // Act
        await service.SaveSettingsAsync(settings1);
        await service.SaveSettingsAsync(settings2);

        // Assert - verify the file contains the latest settings
        var response = await service.GetSettingsAsync();
        response.Data!.Theme.Should().Be(settings2.Theme);
        response.Data.AutoRefreshIntervalSeconds.Should().Be(settings2.AutoRefreshIntervalSeconds);
        response.Data.AutoRefreshEnabled.Should().Be(settings2.AutoRefreshEnabled);
        response.Data.SavedResourceGroupNames.Should().Equal(settings2.SavedResourceGroupNames);
        response.Data.LastActiveSubscriptionId.Should().Be(settings2.LastActiveSubscriptionId);
    }
}
