import { Injectable } from '@angular/core';
import Cookies from 'js-cookie';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly ID_TOKEN_KEY = 'id_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  constructor() {}

  setAccessToken(token: string): void {
    Cookies.set(this.ACCESS_TOKEN_KEY, token, {
      secure: location.protocol === 'https:',
      httpOnly: false, // Client-side access needed for interceptor
      sameSite: 'strict',
      expires: 1 // 1 day
    });
  }

  getAccessToken(): string | undefined {
    return Cookies.get(this.ACCESS_TOKEN_KEY);
  }

  setIdToken(token: string): void {
    Cookies.set(this.ID_TOKEN_KEY, token, {
      secure: location.protocol === 'https:',
      httpOnly: false,
      sameSite: 'strict',
      expires: 1
    });
  }

  getIdToken(): string | undefined {
    return Cookies.get(this.ID_TOKEN_KEY);
  }

  setRefreshToken(token: string): void {
    Cookies.set(this.REFRESH_TOKEN_KEY, token, {
      secure: location.protocol === 'https:',
      httpOnly: false,
      sameSite: 'strict',
      expires: 7 // 7 days
    });
  }

  getRefreshToken(): string | undefined {
    return Cookies.get(this.REFRESH_TOKEN_KEY);
  }

  clearAllTokens(): void {
    Cookies.remove(this.ACCESS_TOKEN_KEY);
    Cookies.remove(this.ID_TOKEN_KEY);
    Cookies.remove(this.REFRESH_TOKEN_KEY);
  }

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
}