#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.AppConfig;

/// <summary>
/// Extension methods for registering AppConfig service dependencies
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the App Configuration service with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The modified service collection for chaining</returns>
    public static IServiceCollection AddAppConfigService(this IServiceCollection services)
    {
        services.AddSingleton<IAppConfigService, AppConfigService>();
        return services;
    }
}
