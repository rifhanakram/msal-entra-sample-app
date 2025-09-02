class AppConfig {
  // Azure AD Configuration
  static const String clientId = 'your-client-id-here';
  static const String tenantId = 'your-tenant-id-here';
  static const String authority = 'https://login.microsoftonline.com/$tenantId';
  
  // API Configuration  
  static const String apiBaseUrl = 'http://localhost:5219/api';
  static const String apiScope = 'api://your-api-client-id/access_as_user';
  
  // Microsoft Graph Configuration
  static const String graphBaseUrl = 'https://graph.microsoft.com/v1.0';
  static const List<String> graphScopes = ['User.Read'];
  
  // Domain restriction
  static const String allowedDomain = 'corzent.com';
  
  // Redirect URIs (for mobile these are typically custom schemes)
  static const String redirectUri = 'msauth://com.example.entra-auth-mobile/callback';
  
  // MSAL Configuration
  static Map<String, dynamic> getMsalConfig() {
    return {
      'client_id': clientId,
      'authority': authority,
      'redirect_uri': redirectUri,
      'broker_redirect_uri_registered': false, // Set to true if using broker
    };
  }
  
  static List<String> getDefaultScopes() {
    return [apiScope, ...graphScopes];
  }
}