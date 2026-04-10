using ConfigCompare.Session.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.Session;

/// <summary>
/// Extension methods for registering session services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the session service with the DI container.
    /// Initializes the database schema eagerly at startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dbPath">Path to the SQLite database file.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddSessionServices(this IServiceCollection services, string dbPath)
    {
        // Register and eagerly initialize DatabaseInitialiser
        services.AddSingleton(sp =>
        {
            var initialiser = new DatabaseInitialiser(dbPath, sp.GetService<Microsoft.Extensions.Logging.ILogger<DatabaseInitialiser>>());
            initialiser.InitialiseAsync().GetAwaiter().GetResult();
            return initialiser;
        });

        // Register ISessionService as scoped
        services.AddScoped<ISessionService>(sp =>
            new SessionService(dbPath, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SessionService>>()));

        return services;
    }
}
