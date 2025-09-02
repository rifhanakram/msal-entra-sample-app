import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:flutter/foundation.dart';
import 'auth_service.dart';
import '../config/app_config.dart';

class ApiService {
  final AuthService _authService;
  
  ApiService(this._authService);
  
  // Get headers with authentication
  Future<Map<String, String>> _getHeaders({bool includeAuth = true}) async {
    final headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    
    if (includeAuth) {
      final token = await _authService.getAccessToken();
      if (token != null) {
        headers['Authorization'] = 'Bearer $token';
      }
    }
    
    return headers;
  }
  
  // Handle HTTP responses
  dynamic _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) {
        return null;
      }
      try {
        return jsonDecode(response.body);
      } catch (e) {
        return response.body;
      }
    } else {
      throw HttpException(
        'HTTP ${response.statusCode}: ${response.body}',
        uri: response.request?.url,
      );
    }
  }
  
  // Generic GET request
  Future<dynamic> get(String endpoint) async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.apiBaseUrl}$endpoint');
      
      debugPrint('API GET: $url');
      
      final response = await http.get(url, headers: headers);
      return _handleResponse(response);
    } catch (e) {
      debugPrint('API GET Error: $e');
      rethrow;
    }
  }
  
  // Generic POST request
  Future<dynamic> post(String endpoint, {Map<String, dynamic>? body}) async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.apiBaseUrl}$endpoint');
      
      debugPrint('API POST: $url');
      
      final response = await http.post(
        url,
        headers: headers,
        body: body != null ? jsonEncode(body) : null,
      );
      
      return _handleResponse(response);
    } catch (e) {
      debugPrint('API POST Error: $e');
      rethrow;
    }
  }
  
  // Generic PUT request
  Future<dynamic> put(String endpoint, {Map<String, dynamic>? body}) async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.apiBaseUrl}$endpoint');
      
      debugPrint('API PUT: $url');
      
      final response = await http.put(
        url,
        headers: headers,
        body: body != null ? jsonEncode(body) : null,
      );
      
      return _handleResponse(response);
    } catch (e) {
      debugPrint('API PUT Error: $e');
      rethrow;
    }
  }
  
  // Generic DELETE request
  Future<dynamic> delete(String endpoint) async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.apiBaseUrl}$endpoint');
      
      debugPrint('API DELETE: $url');
      
      final response = await http.delete(url, headers: headers);
      return _handleResponse(response);
    } catch (e) {
      debugPrint('API DELETE Error: $e');
      rethrow;
    }
  }
  
  // Test endpoint - calls your API's weather forecast or similar endpoint
  Future<List<dynamic>> getWeatherForecast() async {
    try {
      final data = await get('/Sample/authorized');
      return data is List ? data : [data];
    } catch (e) {
      debugPrint('Weather forecast error: $e');
      rethrow;
    }
  }
  
  // Microsoft Graph API calls
  Future<dynamic> getUserProfile() async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.graphBaseUrl}/me');
      
      debugPrint('Graph API GET: $url');
      
      final response = await http.get(url, headers: headers);
      return _handleResponse(response);
    } catch (e) {
      debugPrint('Get user profile error: $e');
      rethrow;
    }
  }
  
  // Get user photo
  Future<String?> getUserPhotoUrl() async {
    try {
      final headers = await _getHeaders();
      final url = Uri.parse('${AppConfig.graphBaseUrl}/me/photo/\$value');
      
      debugPrint('Graph API GET Photo: $url');
      
      final response = await http.get(url, headers: headers);
      
      if (response.statusCode == 200 && response.bodyBytes.isNotEmpty) {
        // Convert bytes to base64 for display
        final base64Image = base64Encode(response.bodyBytes);
        return 'data:image/jpeg;base64,$base64Image';
      }
      
      return null;
    } catch (e) {
      debugPrint('Get user photo error: $e');
      return null;
    }
  }
}