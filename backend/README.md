# Entra Auth API

A .NET Core Web API with Azure Entra ID (formerly Azure AD) authentication for the Corzent.com tenant.

## Features

- JWT Bearer authentication with Azure Entra ID
- CORS configuration for Angular frontend
- Authorized sample endpoint that returns user claims
- Configurable via appsettings.json

## Setup

1. **Configure Azure Entra ID App Registration:**
   - Create an API app registration in Azure Entra ID for the Corzent.com tenant
   - Set the Application ID URI (e.g., `api://your-api-client-id`)
   - Note the Client ID

2. **Update Configuration:**
   Update `appsettings.json` with your actual values:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "corzent.com",
       "TenantId": "corzent.com",
       "ClientId": "your-actual-api-client-id"
     }
   }
   ```

3. **Configure Angular App:**
   In your Angular app's MSAL configuration, add this API as a protected resource:
   ```typescript
   protectedResourceMap: new Map([
     ['https://localhost:7000/api/', ['api://your-api-client-id/access_as_user']]
   ])
   ```

## Running the API

```bash
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7000
- HTTP: http://localhost:5000

### Swagger/OpenAPI Documentation

In development mode, the API includes Swagger UI for interactive API documentation and testing:

- **Swagger UI**: https://localhost:7000 (served at the root in development)
- **OpenAPI JSON**: https://localhost:7000/swagger/v1/swagger.json

The Swagger UI includes:
- Complete API documentation with descriptions
- JWT Bearer token authentication support
- Interactive endpoint testing
- Request/response schemas and examples

#### Using Swagger with Authentication

1. Click the "Authorize" button in Swagger UI
2. Enter your JWT token in the format: `Bearer your-jwt-token-here`
3. Test the authorized endpoints directly from the UI

## Endpoints

### GET /api/sample/authorized
- **Description:** Protected endpoint that returns user information and claims
- **Authentication:** Required (Bearer token)
- **Response:** JSON object with user data, claims, and timestamp

Example response:
```json
{
  "message": "This is authorized data from the API",
  "user": "user@corzent.com",
  "claims": [
    {"type": "aud", "value": "api://your-api-client-id"},
    {"type": "iss", "value": "https://login.microsoftonline.com/tenant-id/v2.0"},
    {"type": "name", "value": "User Name"}
  ],
  "timestamp": "2025-01-15T10:30:00.000Z"
}
```

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:4200` (Angular dev server)

To add more origins, update the `Cors:AllowedOrigins` array in `appsettings.json`.

## Authentication Flow

1. User authenticates with Angular app using MSAL + PKCE
2. Angular app receives access token for the API
3. Angular app includes token in Authorization header: `Bearer <token>`
4. API validates token with Azure Entra ID
5. API returns protected data if token is valid

## Token Validation

The API validates:
- Token signature using Azure Entra ID public keys
- Token issuer (must be from Corzent.com tenant)
- Token audience (must match API client ID)
- Token expiration
- Token not before claims

## Development Features

### Swagger/OpenAPI Integration
- Full OpenAPI 3.0 specification
- XML documentation comments included in Swagger
- JWT Bearer authentication scheme configured
- Detailed response models and examples
- Interactive testing capability

### Response Models
The API uses strongly-typed response models:
- `AuthorizedDataResponse`: Main response for authorized endpoint
- `ClaimInfo`: JWT token claim information

### Logging
- Structured logging with Serilog-compatible format
- Request/response logging for debugging
- User activity tracking for authorized endpoints

## Building and Testing

```bash
# Build the project
dotnet build

# Run tests (if any)
dotnet test

# Run with hot reload for development
dotnet watch run
```