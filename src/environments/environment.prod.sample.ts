// IMPORTANT: Rename this file to 'environment.prod.ts' after configuring your values
// This sample file shows the required configuration structure for production MSAL authentication

export const environment = {
  production: true,
  msalConfig: {
    auth: {
      // Replace with your Azure App Registration client ID
      // This should be the SAME client ID as your development environment
      clientId: 'your-client-id-here',
      
      // Replace with your tenant authority URL
      // Format: https://login.microsoftonline.com/YOUR-TENANT-ID
      // This should be the SAME tenant ID as your development environment
      authority: 'https://login.microsoftonline.com/your-tenant-id-here',
      
      // The PRODUCTION URL where users will be redirected after authentication
      // Make sure this EXACT URL is registered in your Azure App Registration > Authentication > Redirect URIs
      // Example: https://yourdomain.com/login
      redirectUri: 'https://your-production-domain.com/login',
      
      // The PRODUCTION URL where users will be redirected after logout
      // Example: https://yourdomain.com/
      postLogoutRedirectUri: 'https://your-production-domain.com/',
    }
  }
};

// Instructions for Production:
// 1. Copy this file and rename it to 'environment.prod.ts'
// 2. Replace 'your-client-id-here' with your actual Azure App Registration client ID
// 3. Replace 'your-tenant-id-here' with your actual Azure tenant ID
// 4. Replace 'your-production-domain.com' with your actual production domain
// 5. Ensure ALL URLs use HTTPS in production
// 6. Add your production redirect URIs to Azure App Registration > Authentication
// 7. Test the authentication flow thoroughly in your production environment