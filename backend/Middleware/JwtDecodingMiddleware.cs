using EntraAuthApi.Services;
using System.Text.RegularExpressions;

namespace EntraAuthApi.Middleware;

/// <summary>
/// Middleware that decodes JWT tokens and populates the JWT context service
/// </summary>
public class JwtDecodingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtDecodingMiddleware> _logger;
    
    // Regex pattern to extract Bearer token from Authorization header
    private static readonly Regex BearerTokenPattern = new(@"^Bearer\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public JwtDecodingMiddleware(RequestDelegate next, ILogger<JwtDecodingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to extract and decode JWT token if present
        await TryDecodeJwtTokenAsync(context);
        
        // Continue with the next middleware
        await _next(context);
    }

    private async Task TryDecodeJwtTokenAsync(HttpContext context)
    {
        try
        {
            // Get JWT context service from DI container
            var jwtContextService = context.RequestServices.GetService<IJwtContextService>();
            if (jwtContextService == null)
            {
                _logger.LogWarning("IJwtContextService is not registered in DI container");
                return;
            }

            // Extract token from Authorization header
            var token = ExtractTokenFromHeader(context);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("No Bearer token found in Authorization header for request: {Path}", context.Request.Path);
                return;
            }

            // Populate JWT context
            await jwtContextService.PopulateFromTokenAsync(token);
            
            var userId = jwtContextService.UserContext?.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("JWT context populated successfully for user: {UserId} on path: {Path}", 
                    userId, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request - authentication will handle invalid tokens
            _logger.LogError(ex, "Error occurred while decoding JWT token for request: {Path}", context.Request.Path);
        }
    }

    private string? ExtractTokenFromHeader(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeaders))
        {
            return null;
        }

        var authHeader = authHeaders.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        var match = BearerTokenPattern.Match(authHeader);
        return match.Success ? match.Groups[1].Value : null;
    }
}