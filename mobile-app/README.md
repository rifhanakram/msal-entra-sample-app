# Entra Auth Mobile App

A Flutter mobile application with Azure Entra ID authentication, mirroring the functionality of the Angular web frontend.

## Features

- Azure Entra ID authentication using MSAL
- Domain-restricted access (corzent.com)
- User profile display with Microsoft Graph integration
- Authenticated API calls to the backend
- Secure token storage
- Cross-platform (Android & iOS)

## Prerequisites

- Flutter SDK (latest stable)
- Android Studio / Xcode for mobile development
- Azure Entra ID tenant with app registration
- Running backend API (see backend instructions)

## Setup

### 1. Configure Azure App Registration

Ensure your Azure app registration includes mobile redirect URIs:
- Android: `msauth://com.example.entra-auth-mobile/callback`
- iOS: `msauth.com.example.entra-auth-mobile://auth`

### 2. Configure App

Edit `lib/config/app_config.dart` and replace:
- `your-client-id-here` with your Azure app registration client ID
- `your-tenant-id-here` with your Azure tenant ID
- `your-api-client-id` with your backend API client ID

### 3. Install Dependencies

```bash
flutter pub get
```

## Run the App

### Android
```bash
flutter run -d android
```

### iOS
```bash
flutter run -d ios
```

## Project Structure

```
lib/
├── config/
│   └── app_config.dart          # Configuration constants
├── services/
│   ├── auth_service.dart        # Authentication service
│   └── api_service.dart         # API service for HTTP requests
├── screens/
│   ├── login_screen.dart        # Login UI
│   └── dashboard_screen.dart    # Main dashboard UI
└── main.dart                    # App entry point
```

## Key Dependencies

- `msal_flutter`: Microsoft Authentication Library for Flutter
- `provider`: State management
- `http`: HTTP client for API calls
- `flutter_secure_storage`: Secure token storage

## Authentication Flow

1. User taps "Sign in with Microsoft"
2. MSAL redirects to Azure AD login
3. After successful authentication, tokens are stored securely
4. User is redirected back to the app
5. Domain validation ensures only corzent.com users can access
6. Dashboard loads with user profile and API functionality

## API Integration

The app integrates with:
- **Backend API**: Custom API endpoints with JWT authentication
- **Microsoft Graph**: User profile and photo retrieval

## Platform-Specific Notes

### Android
- Requires INTERNET permission in AndroidManifest.xml
- Custom URL scheme configured for MSAL redirects

### iOS
- URL scheme configured in Info.plist
- Network security settings allow HTTP for local development

## Troubleshooting

1. **Authentication fails**: Check redirect URIs in Azure app registration
2. **API calls fail**: Ensure backend is running and accessible
3. **Build errors**: Run `flutter clean && flutter pub get`