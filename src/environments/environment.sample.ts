// IMPORTANT: Rename this file to 'environment.ts' after configuring your values
// This sample file shows the required configuration structure for the MSAL authentication

export const environment = {
  production: false,
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
  }
};

// Instructions:
// 1. Copy this file and rename it to 'environment.ts'
// 2. Replace 'your-client-id-here' with your actual Azure App Registration client ID
// 3. Replace 'your-tenant-id-here' with your actual Azure tenant ID
// 4. Ensure the redirect URIs match exactly what you've configured in Azure
// 5. For production, create a similar 'environment.prod.ts' file with production URLs