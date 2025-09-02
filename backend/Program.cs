using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EntraAuthApi;
using EntraAuthApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
// Enable PII logging for debugging (ONLY in development)
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

// Add services to the container.
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
var corsConfig = builder.Configuration.GetSection("Cors");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tenantId = azureAdConfig["TenantId"];
        var clientId = azureAdConfig["ClientId"];
        
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{tenantId}/v2.0",
                $"https://sts.windows.net/{tenantId}/"
            },
            ValidAudiences = new[]
            {
                $"api://{clientId}",
                clientId
            },
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateTokenReplay = false,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Let default key resolution handle it - logging will happen in events
                return null;
            }
        };

        // Configure metadata address explicitly
        options.MetadataAddress = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Error}", context.Exception?.Message);
                logger.LogError("Exception details: {Exception}", context.Exception);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT Token validated successfully for user: {User}", 
                    context.Principal?.Identity?.Name ?? "Unknown");
                
                // Log token claims for debugging
                if (context.Principal?.Claims != null)
                {
                    foreach (var claim in context.Principal.Claims.Take(5))
                    {
                        logger.LogDebug("Token claim - {Type}: {Value}", claim.Type, claim.Value);
                    }
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var token = context.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    logger.LogInformation("JWT Token received, analyzing...");
                    TokenDebugHelper.LogTokenDetails(token, logger);
                    
                    // Also fetch and log available keys for comparison
                    _ = Task.Run(async () =>
                    {
                        await TokenDebugHelper.GetOpenIdConfigurationAsync(tenantId, logger);
                    });
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

// Add JWT context services
builder.Services.AddJwtContextServices();

// Add allowed domain configuration
builder.Configuration["AllowedDomain"] = "corzent.com";

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Entra Auth API",
        Version = "v1",
        Description = "A .NET Core Web API with Azure Entra ID authentication for the Corzent.com tenant",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@corzent.com"
        }
    });

    // Include XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Entra Auth API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseJwtDecoding();
app.UseAuthorization();

app.MapControllers();

app.Run();
