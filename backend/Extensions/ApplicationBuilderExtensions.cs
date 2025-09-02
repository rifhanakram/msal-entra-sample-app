using EntraAuthApi.Middleware;

namespace EntraAuthApi.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder to configure JWT-related middleware
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds JWT decoding middleware to the request pipeline
    /// This should be called after UseAuthentication() but before UseAuthorization()
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseJwtDecoding(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtDecodingMiddleware>();
    }
}