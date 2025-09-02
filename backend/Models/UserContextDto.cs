using System.ComponentModel.DataAnnotations;

namespace EntraAuthApi.Models;

/// <summary>
/// User context populated from JWT token claims
/// </summary>
public class UserContextDto
{
    /// <summary>
    /// User's unique identifier from Azure AD (oid claim)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// User's email address (preferred_username or email claim)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// User's display name (name claim)
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Azure AD tenant identifier (tid claim)
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Application client ID that the token was issued for (appid claim)
    /// </summary>
    public string? ApplicationId { get; set; }
    
    /// <summary>
    /// User's roles from the token (roles claim)
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Whether the user belongs to the allowed domain
    /// </summary>
    public bool IsFromAllowedDomain { get; set; }
    
    /// <summary>
    /// User's job title if available (jobTitle claim)
    /// </summary>
    public string? JobTitle { get; set; }
    
    /// <summary>
    /// User's department if available (department claim)
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Token issued at timestamp (iat claim)
    /// </summary>
    public DateTime? IssuedAt { get; set; }
    
    /// <summary>
    /// Token expiration timestamp (exp claim)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Additional custom claims that don't fit standard properties
    /// </summary>
    public Dictionary<string, string> AdditionalClaims { get; set; } = new();
}