using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using EntraAuthApi.Services;
using EntraAuthApi.Models;

namespace EntraAuthApi.Controllers;

/// <summary>
/// Sample controller demonstrating Azure Entra ID authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Sample")]
public class SampleController : ControllerBase
{
    private readonly ILogger<SampleController> _logger;
    private readonly IJwtContextService _jwtContextService;

    public SampleController(ILogger<SampleController> logger, IJwtContextService jwtContextService)
    {
        _logger = logger;
        _jwtContextService = jwtContextService;
    }

    /// <summary>
    /// Gets authorized data including user claims and information
    /// </summary>
    /// <returns>User data with claims and timestamp</returns>
    /// <response code="200">Returns authorized user data</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpGet("authorized")]
    [ProducesResponseType(typeof(AuthorizedDataResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetAuthorizedData()
    {
        var userContext = _jwtContextService.UserContext;
        
        _logger.LogInformation("Authorized endpoint accessed by user: {UserId} ({Email})", 
            userContext?.UserId, userContext?.Email);
        
        var userClaims = _jwtContextService.Claims.Select(c => new ClaimInfo 
        { 
            Type = c.Type, 
            Value = c.Value 
        }).ToList();
        
        var response = new AuthorizedDataResponse
        {
            Message = "This is authorized data from the API",
            User = userContext?.DisplayName ?? userContext?.Email ?? User.Identity?.Name ?? "Unknown",
            Claims = userClaims,
            Timestamp = DateTime.UtcNow,
            // NEW: Populate response DTO with JWT context data
            UserContext = userContext
        };

        return Ok(response);
    }
}

/// <summary>
/// Response model for authorized data endpoint
/// </summary>
public class AuthorizedDataResponse
{
    /// <summary>
    /// Welcome message
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Authenticated user name
    /// </summary>
    [Required]
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// User's JWT token claims
    /// </summary>
    [Required]
    public List<ClaimInfo> Claims { get; set; } = new();

    /// <summary>
    /// Response timestamp in UTC
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Structured user context populated from JWT token
    /// </summary>
    public UserContextDto? UserContext { get; set; }
}

/// <summary>
/// JWT token claim information
/// </summary>
public class ClaimInfo
{
    /// <summary>
    /// Claim type (e.g., 'name', 'email', 'aud')
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Claim value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}
