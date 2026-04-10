using ConfigCompare.Settings.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.Settings.Service;

/// <summary>
/// Extension methods for registering settings services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the settings service with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settingsFilePath">Path to the settings JSON file.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddSettingsServices(this IServiceCollection services, string settingsFilePath)
    {
        services.AddSingleton<ISettingsService>(sp =>
            new SettingsService(settingsFilePath, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SettingsService>>()));
        return services;
    }
}
