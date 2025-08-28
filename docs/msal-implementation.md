# MSAL Implementation Documentation

## Overview

This Angular application implements Microsoft Authentication Library (MSAL) for authenticating users with Microsoft Entra ID (formerly Azure AD). The implementation uses the Authorization Code flow with PKCE (Proof Key for Code Exchange) for secure authentication in a Single Page Application (SPA).

## Architecture

### Core Components

1. **AuthService** - Main authentication service managing MSAL operations
2. **TokenService** - Token storage and validation service
3. **AuthGuard** - Route protection guard
4. **MSAL Configuration** - Central configuration for MSAL settings

## File Structure

```
src/
├── app/
│   ├── auth-config.ts              # MSAL configuration
│   ├── services/
│   │   ├── auth.service.ts         # Main authentication service
│   │   └── token.service.ts        # Token management service
│   ├── guards/
│   │   └── auth.guard.ts           # Route protection
│   ├── components/
│   │   ├── login/
│   │   │   └── login.component.ts  # Login page component
│   │   └── dashboard/
│   │       └── dashboard.component.ts # Protected dashboard
│   └── environments/
│       ├── environment.ts          # Development configuration
│       └── environment.prod.ts     # Production configuration
```

## Configuration Details

### MSAL Configuration (`auth-config.ts`)

```typescript
export const msalConfig: Configuration = {
  auth: {
    clientId: environment.msalConfig.auth.clientId,
    authority: environment.msalConfig.auth.authority,
    redirectUri: environment.msalConfig.auth.redirectUri,
    postLogoutRedirectUri: environment.msalConfig.auth.postLogoutRedirectUri,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: LogLevel, message: string) => {
        console.log(message);
      },
      logLevel: LogLevel.Info,
      piiLoggingEnabled: false,
    },
  },
};
```

**Key Configuration Options:**
- `clientId`: Azure App Registration client ID
- `authority`: Tenant-specific authority URL
- `redirectUri`: Where users are redirected after authentication
- `navigateToLoginRequestUrl`: Set to `true` to navigate to original requested URL
- `cacheLocation`: Uses `sessionStorage` for token caching
- `storeAuthStateInCookie`: Disabled to rely on session storage

### Login Request Configuration

```typescript
export const loginRequest = {
  scopes: ['User.Read'],
  domainHint: 'corzent.com',
  prompt: 'select_account',
};
```

**Parameters:**
- `scopes`: Microsoft Graph permissions requested
- `domainHint`: Pre-fills login with specific domain
- `prompt`: Forces account selection dialog

## Service Implementation

### AuthService (`auth.service.ts`)

The `AuthService` is the central authentication service that:

#### Initialization Process
1. Waits for MSAL instance to initialize
2. Handles any pending redirect responses
3. Checks current authentication state
4. Emits ready state to components

```typescript
private async initialize(): Promise<void> {
  try {
    await this.msalService.instance.initialize();
    await this.handleRedirectResponse();
    
    this.isInitialized = true;
    this.isReadySubject.next(true);
    this.checkAuthenticationState();
  } catch (error) {
    console.error('Failed to initialize MSAL:', error);
    this.isReadySubject.next(false);
  }
}
```

#### Authentication State Management

The service maintains authentication state through RxJS BehaviorSubjects:

- `isAuthenticated$`: Observable authentication status
- `isReady$`: Observable initialization status

Authentication state is determined by:
1. Valid access token exists
2. MSAL account is present

```typescript
private checkAuthenticationState(): void {
  const hasToken = this.tokenService.hasValidToken();
  const hasAccount = this.msalService.instance.getAllAccounts().length > 0;
  this.isAuthenticatedSubject.next(hasToken && hasAccount);
}
```

#### Login Flow

Uses redirect-based authentication:

```typescript
login(): Observable<AuthenticationResult> {
  this.ensureInitialized();
  this.msalService.instance.loginRedirect(loginRequest);
  return new Observable<AuthenticationResult>();
}
```

#### Token Acquisition

Implements silent token acquisition with fallback:

```typescript
getAccessToken(): Observable<string> {
  const account = this.msalService.instance.getAllAccounts()[0];
  const silentRequest: SilentRequest = {
    scopes: loginRequest.scopes,
    account: account
  };

  return from(
    this.msalService.instance.acquireTokenSilent(silentRequest)
      .then((result: AuthenticationResult) => {
        this.handleAuthenticationResult(result);
        return result.accessToken;
      })
      .catch(() => {
        this.msalService.instance.acquireTokenRedirect(loginRequest);
        throw new Error('Redirecting for token acquisition');
      })
  );
}
```

### TokenService (`token.service.ts`)

Manages secure token storage using HTTP-only cookies:

#### Cookie Configuration

```typescript
setAccessToken(token: string): void {
  Cookies.set(this.ACCESS_TOKEN_KEY, token, {
    secure: location.protocol === 'https:',
    httpOnly: false,
    sameSite: 'strict',
    expires: 1 // 1 day
  });
}
```

**Security Features:**
- `secure`: Automatically detects HTTPS vs HTTP
- `sameSite: 'strict'`: Prevents CSRF attacks
- `httpOnly: false`: Allows client-side access for API calls
- Short expiration times (1 day for access tokens, 7 days for refresh tokens)

#### Token Validation

```typescript
hasValidToken(): boolean {
  const token = this.getAccessToken();
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const currentTime = Math.floor(Date.now() / 1000);
    return payload.exp > currentTime;
  } catch (error) {
    return false;
  }
}
```

Validates JWT tokens by:
1. Checking token existence
2. Decoding JWT payload
3. Verifying expiration time

## Route Protection

### AuthGuard (`auth.guard.ts`)

Functional guard protecting authenticated routes:

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated$.pipe(
    map(isAuthenticated => {
      if (isAuthenticated) {
        return true;
      } else {
        router.navigate(['/login']);
        return false;
      }
    })
  );
};
```

### Route Configuration

```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/login' }
];
```

## User Interface Components

### LoginComponent

Features:
- Displays login button with loading states
- Automatically redirects authenticated users to dashboard
- Domain validation (Corzent.com only)
- Error handling and display

```typescript
ngOnInit(): void {
  this.subscription.add(
    this.authService.isReady$.subscribe(isReady => {
      this.isReady = isReady;
      if (isReady) {
        this.subscription.add(
          this.authService.isAuthenticated$.subscribe(isAuthenticated => {
            if (isAuthenticated) {
              if (this.authService.isUserFromCorzentDomain()) {
                this.router.navigate(['/dashboard']);
              } else {
                this.errorMessage = 'Access denied. Only Corzent.com domain users are allowed.';
                this.authService.logout();
              }
            }
          })
        );
      }
    })
  );
}
```

## Azure App Registration Requirements

### Authentication Configuration

1. **Platform Type**: Single-page application (SPA)
2. **Redirect URIs**: 
   - Development: `http://localhost:4200/login`
   - Production: Your production domain
3. **Supported Account Types**: Accounts in this organizational directory only
4. **Implicit Grant**: Disabled (using Authorization Code + PKCE)

### API Permissions

- Microsoft Graph: `User.Read` (delegated permission)

## Security Features

### PKCE (Proof Key for Code Exchange)
- Automatically enabled in MSAL.js v2+
- Protects against authorization code interception attacks
- No client secret required for SPAs

### Token Security
- Tokens stored in secure cookies
- Short expiration times
- Automatic token refresh
- Domain validation for additional access control

### CSRF Protection
- `sameSite: 'strict'` cookie setting
- State parameter validation by MSAL

## Environment Configuration

### Development (`environment.ts`)
```typescript
export const environment = {
  production: false,
  msalConfig: {
    auth: {
      clientId: 'your-client-id',
      authority: 'https://login.microsoftonline.com/your-tenant-id',
      redirectUri: 'http://localhost:4200/login',
      postLogoutRedirectUri: 'http://localhost:4200/',
    }
  }
};
```

### Production (`environment.prod.ts`)
- Update `redirectUri` to production domain
- Ensure HTTPS URLs
- Set `production: true`

## Troubleshooting

### Common Issues

1. **"No redirect response found" Error**
   - Verify redirect URI matches exactly in Azure and code
   - Ensure platform type is set to SPA in Azure
   - Check for trailing slashes in URLs

2. **Cookies Not Being Set**
   - Verify `secure` setting matches protocol (HTTP vs HTTPS)
   - Check browser developer tools for cookie security warnings
   - Ensure `sameSite` setting is compatible with your deployment

3. **Authentication State Not Updating**
   - Check token validation logic
   - Verify MSAL account exists after successful login
   - Review console logs for authentication flow debugging

### Debug Mode

Enable detailed logging by setting `logLevel: LogLevel.Verbose` in MSAL configuration.

## Best Practices

1. **Token Handling**
   - Never store tokens in localStorage
   - Use secure, HTTP-only cookies when possible
   - Implement proper token refresh logic

2. **Error Handling**
   - Implement comprehensive error handling for all auth flows
   - Provide user-friendly error messages
   - Log errors for debugging while avoiding PII

3. **Performance**
   - Use silent token acquisition when possible
   - Cache authentication state appropriately
   - Minimize redirect flows

4. **Security**
   - Regularly rotate client secrets (if using confidential client)
   - Implement proper logout functionality
   - Validate user domains/permissions as needed

## Dependencies

- `@azure/msal-angular`: Angular wrapper for MSAL
- `@azure/msal-browser`: Core MSAL browser library
- `js-cookie`: Cookie management library
- `rxjs`: Reactive extensions for state management

## Version Compatibility

- Angular: 17+
- MSAL Angular: 4.x
- MSAL Browser: 4.x
- Node.js: 18+