using EntraAuthApi.Services;

namespace EntraAuthApi.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register JWT-related services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT context services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddJwtContextServices(this IServiceCollection services)
    {
        // Register JWT context service as scoped (per request)
        services.AddScoped<IJwtContextService, JwtContextService>();
        
        return services;
    }
}