using System.Security.Claims;
using EntraAuthApi.Models;

namespace EntraAuthApi.Services;

/// <summary>
/// Service for accessing JWT token context and claims within the current request
/// </summary>
public interface IJwtContextService
{
    /// <summary>
    /// Gets the user context populated from JWT claims
    /// </summary>
    UserContextDto? UserContext { get; }
    
    /// <summary>
    /// Gets all JWT claims from the current token
    /// </summary>
    IEnumerable<Claim> Claims { get; }
    
    /// <summary>
    /// Gets a specific claim value by type
    /// </summary>
    /// <param name="claimType">The claim type to retrieve</param>
    /// <returns>The claim value or null if not found</returns>
    string? GetClaim(string claimType);
    
    /// <summary>
    /// Gets multiple claim values by type (useful for arrays like roles)
    /// </summary>
    /// <param name="claimType">The claim type to retrieve</param>
    /// <returns>Collection of claim values</returns>
    IEnumerable<string> GetClaims(string claimType);
    
    /// <summary>
    /// Populates the JWT context from the authorization token
    /// This is called by the middleware during request processing
    /// </summary>
    /// <param name="token">The JWT token string</param>
    Task PopulateFromTokenAsync(string token);
    
    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if user has the role</returns>
    bool HasRole(string role);
    
    /// <summary>
    /// Checks if the user belongs to the allowed domain
    /// </summary>
    /// <returns>True if user is from allowed domain</returns>
    bool IsFromAllowedDomain();
}