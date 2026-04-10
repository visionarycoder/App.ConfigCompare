namespace ConfigCompare.Settings.Contract;

/// <summary>
/// Provides operations for retrieving, saving, and resetting user application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Retrieves the current application settings.
    /// If no settings file exists, returns default values.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Settings response with success status and settings data or error details.</returns>
    Task<GetSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves application settings to persistent storage.
    /// Validates that AutoRefreshIntervalSeconds is between 10 and 3600.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Save response with success status and error details if applicable.</returns>
    Task<SaveSettingsResponse> SaveSettingsAsync(UserSettingsDto settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets settings to defaults by deleting the persistent settings file.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reset response with success status and error details if applicable.</returns>
    Task<ResetSettingsResponse> ResetSettingsAsync(CancellationToken cancellationToken = default);
}
