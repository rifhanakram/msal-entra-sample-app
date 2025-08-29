import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AccountInfo, AuthenticationResult, SilentRequest } from '@azure/msal-browser';
import { BehaviorSubject, Observable, from } from 'rxjs';
import { TokenService } from './token.service';
import { loginRequest } from '../auth-config';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$: Observable<boolean> = this.isAuthenticatedSubject.asObservable();
  
  private isReadySubject = new BehaviorSubject<boolean>(false);
  public isReady$: Observable<boolean> = this.isReadySubject.asObservable();
  
  private isInitialized = false;

  constructor(
    private msalService: MsalService,
    private tokenService: TokenService
  ) {
    this.initialize();
  }

  private async initialize(): Promise<void> {
    try {
      // Wait for MSAL to be initialized
      await this.msalService.instance.initialize();
      
      // Handle any pending redirect response
      await this.handleRedirectResponse();
      
      this.isInitialized = true;
      this.isReadySubject.next(true);
      this.checkAuthenticationState();
    } catch (error) {
      console.error('Failed to initialize MSAL:', error);
      this.isReadySubject.next(false);
    }
  }

  private async handleRedirectResponse(): Promise<void> {
    try {
      console.log('Checking for redirect response...');
      const response = await this.msalService.instance.handleRedirectPromise();
      if (response) {
        console.log('Redirect response received:', response);
        this.handleAuthenticationResult(response);
      } else {
        console.log('No redirect response found');
      }
    } catch (error) {
      console.error('Error handling redirect response:', error);
    }
  }

  private checkAuthenticationState(): void {
    if (!this.isInitialized) {
      return;
    }
    
    const hasToken = this.tokenService.hasValidToken();
    const hasAccount = this.msalService.instance.getAllAccounts().length > 0;
    const accounts = this.msalService.instance.getAllAccounts();
    
    console.log('Checking auth state:');
    console.log('- Has valid token:', hasToken);
    console.log('- Has account:', hasAccount);
    console.log('- Accounts:', accounts);
    console.log('- Stored access token:', this.tokenService.getAccessToken());
    
    this.isAuthenticatedSubject.next(hasToken && hasAccount);
  }

  private ensureInitialized(): void {
    if (!this.isInitialized) {
      throw new Error('MSAL is not yet initialized. Please wait for initialization to complete.');
    }
  }

  login(): Observable<AuthenticationResult> {
    this.ensureInitialized();
    
    // For redirect flow, we just redirect and the response will be handled when the page loads
    this.msalService.instance.loginRedirect(loginRequest);
    
    // Return an observable that will never emit since we're redirecting
    return new Observable<AuthenticationResult>();
  }

  logout(): void {
    this.ensureInitialized();
    
    this.tokenService.clearAllTokens();
    this.msalService.instance.logoutRedirect();
    this.isAuthenticatedSubject.next(false);
  }

  getAccessToken(): Observable<string> {
    this.ensureInitialized();
    
    const account = this.msalService.instance.getAllAccounts()[0];
    if (!account) {
      throw new Error('No account found');
    }

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
          // For redirect flow, we need to handle the response after redirect
          this.msalService.instance.acquireTokenRedirect(loginRequest);
          throw new Error('Redirecting for token acquisition');
        })
    );
  }


  getCurrentUser(): AccountInfo | null {
    this.ensureInitialized();
    
    const accounts = this.msalService.instance.getAllAccounts();
    return accounts.length > 0 ? accounts[0] : null;
  }

  private handleAuthenticationResult(result: AuthenticationResult): void {
    console.log('Handling auth result:', result);
    if (result.accessToken) {
      console.log('Setting access token');
      this.tokenService.setAccessToken(result.accessToken);
    }
    if (result.idToken) {
      console.log('Setting ID token');
      this.tokenService.setIdToken(result.idToken);
    }
    console.log('Setting authenticated state to true');
    this.isAuthenticatedSubject.next(true);
  }

  isUserFromCorzentDomain(): boolean {
    const user = this.getCurrentUser();
    return user?.username?.endsWith('@corzent.com') || false;
  }
}