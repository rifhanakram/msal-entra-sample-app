// IMPORTANT: Rename this file to 'environment.ts' after configuring your values
// This sample file shows the required configuration structure for the MSAL authentication

export const environment = {
  production: false,
  apiConfig: {
    // Backend API base URL for development
    baseUrl: 'http://localhost:5219/api',
    // Backend API scope for MSAL token acquisition
    scope: 'api://your-api-client-id/access_as_user'
  },
  msalConfig: {
    auth: {
      // Replace with your Azure App Registration client ID
      // You can find this in Azure Portal > App registrations > Your app > Overview > Application (client) ID
      clientId: 'your-client-id-here',
      
      // Replace with your tenant authority URL
      // Format: https://login.microsoftonline.com/YOUR-TENANT-ID
      // You can find your tenant ID in Azure Portal > Entra ID > Overview > Tenant ID
      authority: 'https://login.microsoftonline.com/your-tenant-id-here',
      
      // The URL where users will be redirected after authentication
      // For development, this should match your local dev server
      // Make sure this EXACT URL is registered in your Azure App Registration > Authentication > Redirect URIs
      redirectUri: 'http://localhost:4200/login',
      
      // The URL where users will be redirected after logout
      postLogoutRedirectUri: 'http://localhost:4200/',
    }
  },
  microsoftGraph: {
    // Microsoft Graph API base URL
    baseUrl: 'https://graph.microsoft.com/v1.0',
    // Microsoft Graph API scopes
    scopes: ['User.Read']
  }
};

// Instructions:
// 1. Copy this file and rename it to 'environment.ts'
// 2. Replace 'your-client-id-here' with your actual Azure App Registration client ID
// 3. Replace 'your-tenant-id-here' with your actual Azure tenant ID
// 4. Replace 'your-api-client-id' in the API scope with your backend API client ID
// 5. Ensure the redirect URIs match exactly what you've configured in Azure
// 6. For production, create a similar 'environment.prod.ts' file with production URLs