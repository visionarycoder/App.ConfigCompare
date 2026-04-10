using Azure.Core;
using Azure.Identity;
using ConfigCompare.AzureIdentity.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.AzureIdentity;

/// <summary>
/// Dependency injection extensions for AzureIdentity services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AzureIdentity services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureIdentityServices(this IServiceCollection services)
    {
        // Register DefaultAzureCredential as singleton
        services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());

        // Register HttpClient for Graph API calls
        services.AddHttpClient<AuthService>();

        // Register IAuthService
        services.AddSingleton<IAuthService, AuthService>();

        return services;
    }
}
