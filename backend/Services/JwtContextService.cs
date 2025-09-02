using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EntraAuthApi.Models;

namespace EntraAuthApi.Services;

/// <summary>
/// Implementation of JWT context service for accessing token claims
/// </summary>
public class JwtContextService : IJwtContextService
{
    private readonly ILogger<JwtContextService> _logger;
    private readonly IConfiguration _configuration;
    private UserContextDto? _userContext;
    private List<Claim> _claims = new();
    
    // Standard JWT claim types
    private const string ObjectIdClaim = "oid";
    private const string TenantIdClaim = "tid";
    private const string ApplicationIdClaim = "appid";
    private const string NameClaim = "name";
    private const string PreferredUsernameClaim = "preferred_username";
    private const string EmailClaim = "email";
    private const string RolesClaim = "roles";
    private const string JobTitleClaim = "jobTitle";
    private const string DepartmentClaim = "department";
    private const string IssuedAtClaim = "iat";
    private const string ExpirationClaim = "exp";
    
    public JwtContextService(ILogger<JwtContextService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    /// <inheritdoc />
    public UserContextDto? UserContext => _userContext;
    
    /// <inheritdoc />
    public IEnumerable<Claim> Claims => _claims;
    
    /// <inheritdoc />
    public string? GetClaim(string claimType)
    {
        return _claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
    
    /// <inheritdoc />
    public IEnumerable<string> GetClaims(string claimType)
    {
        return _claims.Where(c => c.Type == claimType).Select(c => c.Value);
    }
    
    /// <inheritdoc />
    public async Task PopulateFromTokenAsync(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            _claims = jsonToken.Claims.ToList();
            
            _userContext = await BuildUserContextAsync(jsonToken);
            
            _logger.LogDebug("JWT context populated for user: {UserId}", _userContext.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to populate JWT context from token");
            _userContext = null;
            _claims.Clear();
        }
    }
    
    /// <inheritdoc />
    public bool HasRole(string role)
    {
        return _userContext?.Roles.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
    }
    
    /// <inheritdoc />
    public bool IsFromAllowedDomain()
    {
        return _userContext?.IsFromAllowedDomain ?? false;
    }
    
    private async Task<UserContextDto> BuildUserContextAsync(JwtSecurityToken token)
    {
        var claims = token.Claims.ToDictionary(c => c.Type, c => c.Value, StringComparer.OrdinalIgnoreCase);
        
        var userContext = new UserContextDto
        {
            UserId = GetClaimValue(claims, ObjectIdClaim),
            Email = GetClaimValue(claims, PreferredUsernameClaim) ?? GetClaimValue(claims, EmailClaim),
            DisplayName = GetClaimValue(claims, NameClaim),
            TenantId = GetClaimValue(claims, TenantIdClaim),
            ApplicationId = GetClaimValue(claims, ApplicationIdClaim),
            JobTitle = GetClaimValue(claims, JobTitleClaim),
            Department = GetClaimValue(claims, DepartmentClaim),
            IssuedAt = ConvertUnixTimestamp(GetClaimValue(claims, IssuedAtClaim)),
            ExpiresAt = ConvertUnixTimestamp(GetClaimValue(claims, ExpirationClaim))
        };
        
        // Handle roles (can be array or single value)
        var rolesClaims = token.Claims.Where(c => c.Type.Equals(RolesClaim, StringComparison.OrdinalIgnoreCase));
        userContext.Roles = rolesClaims.Select(c => c.Value).ToList();
        
        // Determine if user is from allowed domain
        userContext.IsFromAllowedDomain = await IsFromAllowedDomainAsync(userContext.Email);
        
        // Add additional custom claims
        var standardClaims = new[]
        {
            ObjectIdClaim, TenantIdClaim, ApplicationIdClaim, NameClaim, 
            PreferredUsernameClaim, EmailClaim, RolesClaim, JobTitleClaim, 
            DepartmentClaim, IssuedAtClaim, ExpirationClaim,
            "iss", "aud", "sub", "nbf", "exp", "iat", "auth_time", "ver"
        };
        
        foreach (var claim in token.Claims)
        {
            if (!standardClaims.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            {
                userContext.AdditionalClaims[claim.Type] = claim.Value;
            }
        }
        
        return userContext;
    }
    
    private static string? GetClaimValue(Dictionary<string, string> claims, string claimType)
    {
        return claims.TryGetValue(claimType, out var value) ? value : null;
    }
    
    private static DateTime? ConvertUnixTimestamp(string? timestamp)
    {
        if (string.IsNullOrEmpty(timestamp) || !long.TryParse(timestamp, out var unixTime))
            return null;
            
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
    }
    
    private async Task<bool> IsFromAllowedDomainAsync(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        // Get allowed domain from configuration
        var allowedDomain = _configuration["AllowedDomain"] ?? "corzent.com";
        
        return email.EndsWith($"@{allowedDomain}", StringComparison.OrdinalIgnoreCase);
    }
}