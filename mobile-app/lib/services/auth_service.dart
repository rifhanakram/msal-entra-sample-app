import 'package:flutter/foundation.dart';
import 'package:msal_flutter/msal_flutter.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/app_config.dart';

class AuthService extends ChangeNotifier {
  PublicClientApplication? _pca;
  bool _isInitialized = false;
  bool _isAuthenticated = false;
  String? _accessToken;
  String? _idToken;
  String? _currentUserIdentifier;
  String? _currentUsername;
  String? _errorMessage;
  
  final FlutterSecureStorage _secureStorage = const FlutterSecureStorage();
  
  // Getters
  bool get isInitialized => _isInitialized;
  bool get isAuthenticated => _isAuthenticated;
  String? get accessToken => _accessToken;
  String? get idToken => _idToken;
  String? get currentUserIdentifier => _currentUserIdentifier;
  String? get currentUsername => _currentUsername;
  String? get errorMessage => _errorMessage;
  
  // Initialize MSAL
  Future<void> initialize() async {
    try {
      _pca = await PublicClientApplication.createPublicClientApplication(
        AppConfig.getMsalConfig(),
      );
      
      _isInitialized = true;
      
      // Check if user is already signed in
      await _checkExistingAccount();
      
      notifyListeners();
    } catch (e) {
      debugPrint('Error initializing MSAL: $e');
      _errorMessage = 'Failed to initialize authentication: $e';
      notifyListeners();
    }
  }
  
  // Check for existing account and tokens
  Future<void> _checkExistingAccount() async {
    if (_pca == null) return;
    
    try {
      // Try to get cached tokens and user info
      final cachedToken = await _secureStorage.read(key: 'access_token');
      final cachedUsername = await _secureStorage.read(key: 'username');
      final cachedUserIdentifier = await _secureStorage.read(key: 'user_identifier');
      
      if (cachedToken != null && cachedUsername != null) {
        _accessToken = cachedToken;
        _currentUsername = cachedUsername;
        _currentUserIdentifier = cachedUserIdentifier;
        _isAuthenticated = _isUserFromAllowedDomain();
      }
    } catch (e) {
      debugPrint('Error checking existing account: $e');
    }
  }
  
  // Sign in
  Future<bool> signIn() async {
    if (_pca == null) {
      _errorMessage = 'MSAL not initialized';
      notifyListeners();
      return false;
    }
    
    try {
      _errorMessage = null;
      notifyListeners();
      
      final result = await _pca!.acquireToken(AppConfig.getDefaultScopes());
      
      if (result != null) {
        _currentUsername = result.account?.username;
        _currentUserIdentifier = result.account?.identifier;
        _accessToken = result.accessToken;
        _idToken = result.idToken;
        
        // Store tokens and user info securely
        await _secureStorage.write(key: 'access_token', value: _accessToken);
        await _secureStorage.write(key: 'id_token', value: _idToken);
        if (_currentUsername != null) {
          await _secureStorage.write(key: 'username', value: _currentUsername);
        }
        if (_currentUserIdentifier != null) {
          await _secureStorage.write(key: 'user_identifier', value: _currentUserIdentifier);
        }
        
        // Check domain restriction
        if (_isUserFromAllowedDomain()) {
          _isAuthenticated = true;
          notifyListeners();
          return true;
        } else {
          _errorMessage = 'Access denied. Only ${AppConfig.allowedDomain} domain users are allowed.';
          await signOut();
          return false;
        }
      }
      
      return false;
    } catch (e) {
      debugPrint('Sign in error: $e');
      _errorMessage = 'Sign in failed: $e';
      notifyListeners();
      return false;
    }
  }
  
  // Sign out
  Future<void> signOut() async {
    if (_pca == null) return;
    
    try {
      // Use signOut method instead of removeAccount for version 2.0.0
      await _pca!.signOut();
      
      // Clear stored tokens and user info
      await _secureStorage.delete(key: 'access_token');
      await _secureStorage.delete(key: 'id_token');
      await _secureStorage.delete(key: 'username');
      await _secureStorage.delete(key: 'user_identifier');
      
      // Clear state
      _currentUsername = null;
      _currentUserIdentifier = null;
      _accessToken = null;
      _idToken = null;
      _isAuthenticated = false;
      _errorMessage = null;
      
      notifyListeners();
    } catch (e) {
      debugPrint('Sign out error: $e');
    }
  }
  
  // Get access token (refresh if needed)
  Future<String?> getAccessToken({List<String>? scopes}) async {
    if (_pca == null || _currentUserIdentifier == null) return null;
    
    try {
      final result = await _pca!.acquireTokenSilent(
        scopes ?? AppConfig.getDefaultScopes(),
        _currentUserIdentifier!,
      );
      
      if (result != null) {
        _accessToken = result.accessToken;
        await _secureStorage.write(key: 'access_token', value: _accessToken);
        return _accessToken;
      }
    } catch (e) {
      debugPrint('Error getting access token: $e');
      // If silent token acquisition fails, user needs to sign in again
      _isAuthenticated = false;
      notifyListeners();
    }
    
    return null;
  }
  
  // Check if user is from allowed domain
  bool _isUserFromAllowedDomain() {
    if (_currentUsername == null) return false;
    return _currentUsername!.endsWith('@${AppConfig.allowedDomain}');
  }
  
  // Get user display name
  String? getUserDisplayName() {
    return _currentUsername ?? _currentUserIdentifier;
  }
  
  // Clear error message
  void clearError() {
    _errorMessage = null;
    notifyListeners();
  }
}