using Azure.Core;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.ResourceGroup;

/// <summary>
/// Extension methods for registering ResourceGroup services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ResourceGroup services to the dependency injection container.
    /// Requires TokenCredential to be registered as a singleton.
    /// </summary>
    public static IServiceCollection AddResourceGroupServices(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var credential = sp.GetRequiredService<TokenCredential>();
            return new ArmClient(credential);
        });

        services.AddScoped<IResourceGroupService, ResourceGroupService>();

        return services;
    }
}
