using System.Text.Json;
using ConfigCompare.Settings.Contract;
using Microsoft.Extensions.Logging;

namespace ConfigCompare.Settings.Service;

/// <summary>
/// JSON-file-backed implementation of ISettingsService.
/// Persists settings to a JSON file at the specified path.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string SettingsFilePath;
    private readonly ILogger<SettingsService> Logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Default settings returned when no file exists.
    /// </summary>
    public static UserSettingsDto DefaultSettings => new(
        Theme: AppTheme.System,
        AutoRefreshIntervalSeconds: 60,
        AutoRefreshEnabled: false,
        SavedResourceGroupNames: Array.Empty<string>(),
        LastActiveSubscriptionId: null);

    /// <summary>
    /// Initializes a new instance of the SettingsService.
    /// </summary>
    /// <param name="settingsFilePath">Path to the JSON settings file.</param>
    /// <param name="logger">Logger instance.</param>
    public SettingsService(string settingsFilePath, ILogger<SettingsService> logger)
    {
        SettingsFilePath = settingsFilePath ?? throw new ArgumentNullException(nameof(settingsFilePath));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the current application settings.
    /// If no settings file exists, returns default values.
    /// </summary>
    public async Task<GetSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                Logger.LogInformation("Settings file does not exist at '{FilePath}'; returning defaults.", SettingsFilePath);
                return new GetSettingsResponse(true, null, DefaultSettings);
            }

            var json = await File.ReadAllTextAsync(SettingsFilePath, cancellationToken).ConfigureAwait(false);
            var settings = JsonSerializer.Deserialize<UserSettingsDto>(json, JsonOptions);

            if (settings is null)
            {
                Logger.LogWarning("Deserialized settings was null; returning defaults.");
                return new GetSettingsResponse(true, null, DefaultSettings);
            }

            Logger.LogInformation("Successfully loaded settings from '{FilePath}'.", SettingsFilePath);
            return new GetSettingsResponse(true, null, settings);
        }
        catch (JsonException ex)
        {
            const string errorMessage = "Settings file contains invalid JSON.";
            Logger.LogWarning(ex, errorMessage);
            return new GetSettingsResponse(false, errorMessage, null);
        }
        catch (IOException ex)
        {
            var errorMessage = $"Failed to read settings file: {ex.Message}";
            Logger.LogWarning(ex, errorMessage);
            return new GetSettingsResponse(false, errorMessage, null);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error reading settings: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new GetSettingsResponse(false, errorMessage, null);
        }
    }

    /// <summary>
    /// Saves application settings to persistent storage.
    /// Validates that AutoRefreshIntervalSeconds is between 10 and 3600.
    /// </summary>
    public async Task<SaveSettingsResponse> SaveSettingsAsync(UserSettingsDto settings, CancellationToken cancellationToken = default)
    {
        try
        {
            if (settings is null)
            {
                const string errorMessage = "Settings cannot be null.";
                Logger.LogWarning(errorMessage);
                return new SaveSettingsResponse(false, errorMessage);
            }

            // Validate auto-refresh interval
            if (settings.AutoRefreshIntervalSeconds < 10 || settings.AutoRefreshIntervalSeconds > 3600)
            {
                var errorMessage = $"AutoRefreshIntervalSeconds must be between 10 and 3600; got {settings.AutoRefreshIntervalSeconds}.";
                Logger.LogWarning(errorMessage);
                return new SaveSettingsResponse(false, errorMessage);
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Logger.LogInformation("Created settings directory at '{Directory}'.", directory);
            }

            // Write to temp file then atomically move
            var tempFilePath = SettingsFilePath + ".tmp";
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json, cancellationToken).ConfigureAwait(false);

            File.Move(tempFilePath, SettingsFilePath, overwrite: true);
            Logger.LogInformation("Successfully saved settings to '{FilePath}'.", SettingsFilePath);

            return new SaveSettingsResponse(true, null);
        }
        catch (IOException ex)
        {
            var errorMessage = $"Failed to write settings file: {ex.Message}";
            Logger.LogWarning(ex, errorMessage);
            return new SaveSettingsResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error saving settings: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new SaveSettingsResponse(false, errorMessage);
        }
    }

    /// <summary>
    /// Resets settings to defaults by deleting the persistent settings file.
    /// </summary>
    public async Task<ResetSettingsResponse> ResetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                File.Delete(SettingsFilePath);
                Logger.LogInformation("Deleted settings file at '{FilePath}'.", SettingsFilePath);
            }
            else
            {
                Logger.LogInformation("Settings file did not exist; nothing to delete.");
            }

            return new ResetSettingsResponse(true, null);
        }
        catch (IOException ex)
        {
            var errorMessage = $"Failed to delete settings file: {ex.Message}";
            Logger.LogWarning(ex, errorMessage);
            return new ResetSettingsResponse(false, errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error resetting settings: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new ResetSettingsResponse(false, errorMessage);
        }
    }
}
