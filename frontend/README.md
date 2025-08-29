# Entra Auth Angular Sample

An Angular application implementing OAuth PKCE flow using MSAL for Microsoft Entra ID authentication, specifically configured for users with `corzent.com` domain.

## Features

- **OAuth PKCE Flow**: Secure authentication using Proof Key for Code Exchange
- **Microsoft Entra ID Integration**: Uses MSAL (Microsoft Authentication Library) for Angular
- **Domain Restriction**: Only allows users with `@corzent.com` email addresses
- **Secure Token Storage**: JWT tokens stored securely in HTTP-only cookies
- **HTTP Interceptor**: Automatically adds Bearer tokens to API requests
- **Route Protection**: Auth guards protect protected routes

## Prerequisites

- Node.js 18+ 
- Angular CLI
- Microsoft Entra ID tenant with app registration

## Setup Instructions

### 1. Microsoft Entra ID App Registration

1. Navigate to [Azure Portal](https://portal.azure.com/)
2. Go to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `Entra Auth Angular Sample`
   - **Supported account types**: `Accounts in this organizational directory only`
   - **Redirect URI**: 
     - Platform: `Single-page application (SPA)`
     - URL: `http://localhost:4200` (for development)
5. After creation, note down:
   - **Application (client) ID**
   - **Directory (tenant) ID**

### 2. Configure App Registration

1. In your app registration, go to **Authentication**
2. Under **Single-page application**, add redirect URIs:
   - `http://localhost:4200` (development)
   - Your production URL (when deploying)
3. Under **Advanced settings**:
   - Enable **Access tokens** and **ID tokens**
   - Set **Allow public client flows** to **Yes**

### 3. Update Environment Configuration

1. Open `src/environments/environment.ts`
2. Replace placeholders:
   ```typescript
   export const environment = {
     production: false,
     msalConfig: {
       auth: {
         clientId: 'YOUR_APPLICATION_CLIENT_ID', // From step 1
         authority: 'https://login.microsoftonline.com/YOUR_TENANT_ID', // From step 1
         redirectUri: 'http://localhost:4200',
         postLogoutRedirectUri: 'http://localhost:4200',
       }
     }
   };
   ```

3. Update `src/environments/environment.prod.ts` with production URLs

### 4. Install and Run

```bash
# Install dependencies
npm install

# Start development server
ng serve

# Navigate to http://localhost:4200
```

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── login/           # Login component
│   │   └── dashboard/       # Protected dashboard
│   ├── guards/
│   │   └── auth.guard.ts    # Route protection
│   ├── interceptors/
│   │   └── auth.interceptor.ts  # HTTP interceptor for JWT
│   ├── services/
│   │   ├── auth.service.ts      # Authentication logic
│   │   ├── http.service.ts      # HTTP service wrapper
│   │   └── token.service.ts     # Secure token management
│   ├── auth-config.ts       # MSAL configuration
│   └── app.config.ts        # App configuration
└── environments/            # Environment configurations
```

## Key Components

### Authentication Flow

1. **Login**: Users click "Sign in with Microsoft" 
2. **Redirect**: Redirected to Microsoft Entra ID login
3. **Domain Check**: After successful login, checks if user email ends with `@corzent.com`
4. **Token Storage**: Access and ID tokens stored securely in cookies
5. **Route Protection**: Auth guard protects dashboard route

### Security Features

- **PKCE Flow**: Uses Proof Key for Code Exchange for enhanced security
- **Secure Cookies**: Tokens stored with `secure`, `httpOnly`, and `sameSite` flags
- **Domain Validation**: Only allows specific domain users
- **Automatic Token Refresh**: Silent token refresh before expiration
- **HTTP Interceptor**: Automatically adds Bearer token to API requests

## Usage

### Login
Navigate to `/login` and click "Sign in with Microsoft". Only users with `@corzent.com` domain will be allowed access.

### Dashboard  
After successful authentication, users are redirected to `/dashboard` where they can:
- View their profile information
- Test authenticated API calls
- Logout

### API Calls
The HTTP service automatically includes Bearer tokens in requests:
```typescript
constructor(private httpService: HttpService) {}

// This will automatically include the Bearer token
this.httpService.get('https://graph.microsoft.com/v1.0/me').subscribe(user => {
  console.log(user);
});
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `clientId` | Entra ID Application ID | `12345678-1234-1234-1234-123456789012` |
| `authority` | Entra ID Authority URL | `https://login.microsoftonline.com/your-tenant-id` |
| `redirectUri` | Application redirect URL | `http://localhost:4200` |
| `postLogoutRedirectUri` | Post logout redirect URL | `http://localhost:4200` |

## Production Deployment

1. Update `environment.prod.ts` with production URLs
2. Build for production: `ng build --prod`
3. Update Entra ID app registration with production redirect URIs
4. Deploy the `dist/` folder to your hosting provider

## Troubleshooting

### Common Issues

1. **Login fails**: Check if redirect URI in Entra ID matches your application URL
2. **Access denied**: Ensure user email ends with `@corzent.com`
3. **Token issues**: Check browser cookies and clear if necessary
4. **CORS errors**: Ensure proper CORS configuration in your APIs

### Debug Mode

Enable debug logging by updating `auth-config.ts`:
```typescript
system: {
  loggerOptions: {
    logLevel: LogLevel.Verbose, // Change to Verbose for detailed logs
  },
},
```

## Security Considerations

- Never commit sensitive credentials to version control
- Use environment variables for production configuration  
- Implement proper CORS policies
- Regularly rotate client secrets (if using confidential client)
- Monitor authentication logs in Azure

## License

MIT License - see LICENSE file for details.