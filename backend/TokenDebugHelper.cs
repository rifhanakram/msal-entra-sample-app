using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace EntraAuthApi;

public static class TokenDebugHelper
{
    public static void LogTokenDetails(string token, ILogger logger)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            logger.LogInformation("=== JWT Token Analysis ===");
            logger.LogInformation("Algorithm: {Alg}", jsonToken.Header.Alg);
            logger.LogInformation("Key ID (kid): {Kid}", jsonToken.Header.Kid);
            logger.LogInformation("Type: {Typ}", jsonToken.Header.Typ);
            logger.LogInformation("Issuer: {Iss}", jsonToken.Issuer);
            logger.LogInformation("Audiences: {Aud}", string.Join(", ", jsonToken.Audiences));
            logger.LogInformation("Valid From: {ValidFrom}", jsonToken.ValidFrom.ToString("O"));
            logger.LogInformation("Valid To: {ValidTo}", jsonToken.ValidTo.ToString("O"));
            logger.LogInformation("Subject: {Sub}", jsonToken.Subject);
            
            // Log some key claims
            var claims = jsonToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            
            if (claims.ContainsKey("appid"))
                logger.LogInformation("App ID: {AppId}", claims["appid"]);
                
            if (claims.ContainsKey("tid"))
                logger.LogInformation("Tenant ID: {TenantId}", claims["tid"]);
                
            if (claims.ContainsKey("oid"))
                logger.LogInformation("Object ID: {ObjectId}", claims["oid"]);
                
            if (claims.ContainsKey("name"))
                logger.LogInformation("Name: {Name}", claims["name"]);
                
            if (claims.ContainsKey("preferred_username"))
                logger.LogInformation("Username: {Username}", claims["preferred_username"]);
                
            logger.LogInformation("=== End Token Analysis ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to analyze JWT token");
        }
    }
    
    public static async Task<string> GetOpenIdConfigurationAsync(string tenantId, ILogger logger)
    {
        try
        {
            using var httpClient = new HttpClient();
            var configUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
            logger.LogInformation("Fetching OpenID configuration from: {Url}", configUrl);
            
            var response = await httpClient.GetStringAsync(configUrl);
            var config = JsonDocument.Parse(response);
            
            var jwksUri = config.RootElement.GetProperty("jwks_uri").GetString();
            logger.LogInformation("JWKS URI: {JwksUri}", jwksUri);
            
            // Fetch JWKS to see available keys
            var jwksResponse = await httpClient.GetStringAsync(jwksUri);
            var jwks = JsonDocument.Parse(jwksResponse);
            
            logger.LogInformation("Available signing keys:");
            foreach (var key in jwks.RootElement.GetProperty("keys").EnumerateArray())
            {
                var kid = key.TryGetProperty("kid", out var kidProp) ? kidProp.GetString() : "N/A";
                var use = key.TryGetProperty("use", out var useProp) ? useProp.GetString() : "N/A";
                var alg = key.TryGetProperty("alg", out var algProp) ? algProp.GetString() : "N/A";
                logger.LogInformation("Key - kid: {Kid}, use: {Use}, alg: {Alg}", kid, use, alg);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch OpenID configuration");
            return string.Empty;
        }
    }
}