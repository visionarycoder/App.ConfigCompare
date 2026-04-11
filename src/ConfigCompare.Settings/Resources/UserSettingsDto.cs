namespace ConfigCompare.Settings.Resources;

/// <summary>
/// Data transfer object for user application settings.
/// </summary>
/// <param name="Theme">The user's preferred application theme.</param>
/// <param name="AutoRefreshIntervalSeconds">Auto-refresh interval in seconds (10–3600).</param>
/// <param name="AutoRefreshEnabled">Whether auto-refresh is enabled.</param>
/// <param name="SavedResourceGroupNames">List of resource group names to display on startup.</param>
/// <param name="LastActiveSubscriptionId">The last active Azure subscription ID, if any.</param>
/// <param name="AzureConnection">Saved Azure App Configuration connection details, if any.</param>
public record UserSettingsDto(
    AppTheme Theme,
    int AutoRefreshIntervalSeconds,
    bool AutoRefreshEnabled,
    IReadOnlyList<string> SavedResourceGroupNames,
    string? LastActiveSubscriptionId,
    AzureConnectionDto? AzureConnection = null);
