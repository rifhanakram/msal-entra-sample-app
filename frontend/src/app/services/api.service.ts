import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiBaseUrl = environment.apiConfig.baseUrl;

  constructor(private http: HttpClient) {}

  /**
   * Test endpoint - calls your API's weather forecast or similar endpoint
   * The MSAL interceptor will automatically add the token based on protectedResourceMap
   */
  getWeatherForecast(): Observable<any> {
    return this.http.get(`${this.apiBaseUrl}/Sample/authorized`);
  }

  /**
   * Generic method to call any endpoint on your custom API
   */
  get(endpoint: string): Observable<any> {
    return this.http.get(`${this.apiBaseUrl}${endpoint}`);
  }
}