#nullable enable

using ConfigCompare.AppConfig.Resources;

namespace ConfigCompare.AppConfig;

/// <summary>
/// Service for managing Azure App Configuration operations including read, write, find/replace, and copy settings
/// </summary>
public interface IAppConfigService
{
    /// <summary>
    /// Retrieves all configuration items from an App Configuration instance
    /// </summary>
    /// <param name="endpoint">The App Configuration endpoint URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing all configurations or error</returns>
    Task<GetConfigurationsResponse> GetConfigurationsAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates or creates a configuration item in App Configuration
    /// </summary>
    /// <param name="endpoint">The App Configuration endpoint URL</param>
    /// <param name="configuration">The configuration item to update/create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with the updated configuration or error</returns>
    Task<UpdateConfigurationResponse> UpdateConfigurationAsync(
        string endpoint,
        ConfigurationItemDto configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration item from App Configuration
    /// </summary>
    /// <param name="endpoint">The App Configuration endpoint URL</param>
    /// <param name="key">The key of the configuration to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response indicating success or error</returns>
    Task<UpdateConfigurationResponse> DeleteConfigurationAsync(
        string endpoint,
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds and replaces values in multiple configuration items
    /// </summary>
    /// <param name="endpoint">The App Configuration endpoint URL</param>
    /// <param name="findValue">The value to find</param>
    /// <param name="replaceValue">The replacement value</param>
    /// <param name="keysToUpdate">Optional list of specific keys to update. If null, searches all configs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with count of replacements and affected keys</returns>
    Task<FindReplaceResponse> FindAndReplaceAsync(
        string endpoint,
        string findValue,
        string replaceValue,
        List<string>? keysToUpdate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies all settings from a source App Configuration instance to a target instance
    /// </summary>
    /// <param name="sourceEndpoint">The source App Configuration endpoint URL</param>
    /// <param name="targetEndpoint">The target App Configuration endpoint URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with count of copied items and success status</returns>
    Task<CopySettingsResponse> CopySettingsAsync(
        string sourceEndpoint,
        string targetEndpoint,
        CancellationToken cancellationToken = default);
}
