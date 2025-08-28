import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h2>Welcome to Entra Auth App</h2>
        <p>Please sign in with your Corzent.com account</p>
        <button 
          class="login-button" 
          (click)="login()"
          [disabled]="isLoading || !isReady">
          {{ getButtonText() }}
        </button>
        <div class="error-message" *ngIf="errorMessage">
          {{ errorMessage }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background-color: #f5f5f5;
    }
    
    .login-card {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      text-align: center;
      max-width: 400px;
      width: 100%;
    }
    
    h2 {
      color: #333;
      margin-bottom: 1rem;
    }
    
    p {
      color: #666;
      margin-bottom: 2rem;
    }
    
    .login-button {
      background-color: #0078d4;
      color: white;
      border: none;
      padding: 12px 24px;
      border-radius: 4px;
      cursor: pointer;
      font-size: 16px;
      width: 100%;
      transition: background-color 0.2s;
    }
    
    .login-button:hover:not(:disabled) {
      background-color: #106ebe;
    }
    
    .login-button:disabled {
      background-color: #ccc;
      cursor: not-allowed;
    }
    
    .error-message {
      color: #d32f2f;
      margin-top: 1rem;
      padding: 8px;
      background-color: #ffebee;
      border-radius: 4px;
    }
  `]
})
export class LoginComponent implements OnInit, OnDestroy {
  isLoading = false;
  errorMessage = '';
  isReady = false;
  private subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.subscription.add(
      this.authService.isReady$.subscribe(isReady => {
        this.isReady = isReady;
        if (isReady) {
          // Check if user is already authenticated
          this.subscription.add(
            this.authService.isAuthenticated$.subscribe(isAuthenticated => {
              if (isAuthenticated) {
                if (this.authService.isUserFromCorzentDomain()) {
                  this.router.navigate(['/dashboard']);
                } else {
                  this.errorMessage = 'Access denied. Only Corzent.com domain users are allowed.';
                  this.authService.logout();
                }
              }
            })
          );
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  getButtonText(): string {
    if (!this.isReady) {
      return 'Initializing...';
    }
    if (this.isLoading) {
      return 'Signing in...';
    }
    return 'Sign in with Microsoft';
  }

  login(): void {
    if (!this.isReady) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // For redirect flow, we just call login and the page will redirect
    this.authService.login().subscribe({
      next: (result) => {
        // This should not be reached in redirect flow
        console.log('Login result:', result);
      },
      error: (error) => {
        // This should not be reached in redirect flow
        console.error('Login error:', error);
        this.isLoading = false;
      }
    });
  }
}