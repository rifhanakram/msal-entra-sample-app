import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { ApiService } from '../../services/api.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <header class="header">
        <h1>Dashboard</h1>
        <div class="user-info" *ngIf="currentUser">
          <span>Welcome, {{ currentUser.name || currentUser.username }}</span>
          <button class="logout-button" (click)="logout()">Logout</button>
        </div>
      </header>
      
      <main class="main-content">
        <div class="profile-card" *ngIf="userProfile">
          <h2>User Profile</h2>
          <div class="profile-info">
            <p><strong>Name:</strong> {{ userProfile.displayName }}</p>
            <p><strong>Email:</strong> {{ userProfile.userPrincipalName }}</p>
            <p><strong>Job Title:</strong> {{ userProfile.jobTitle || 'N/A' }}</p>
            <p><strong>Department:</strong> {{ userProfile.department || 'N/A' }}</p>
          </div>
        </div>
        
        <div class="api-test">
          <h2>API Tests</h2>
          <button class="test-button" (click)="testGraphApiCall()">Test Graph API Call</button>
          <button class="test-button" (click)="testCustomApiCall()">Test Custom API Call</button>
          <div class="api-result" *ngIf="apiResult">
            <h3>{{ apiResult.title }}</h3>
            <pre>{{ apiResult.data | json }}</pre>
          </div>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      background-color: #f5f5f5;
    }
    
    .header {
      background: white;
      padding: 1rem 2rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    
    .user-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    
    .logout-button {
      background-color: #d32f2f;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      cursor: pointer;
    }
    
    .logout-button:hover {
      background-color: #b71c1c;
    }
    
    .main-content {
      padding: 2rem;
      display: grid;
      gap: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .profile-card, .api-test {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .profile-info p {
      margin: 0.5rem 0;
    }
    
    .test-button {
      background-color: #0078d4;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 4px;
      cursor: pointer;
      margin-bottom: 1rem;
      margin-right: 1rem;
    }
    
    .test-button:hover {
      background-color: #106ebe;
    }
    
    .api-result {
      background-color: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
      overflow-x: auto;
    }
    
    pre {
      margin: 0;
      white-space: pre-wrap;
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentUser: any = null;
  userProfile: any = null;
  apiResult: any = null;
  private subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private httpClient: HttpClient,
    private apiService: ApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.subscription.add(
      this.authService.isReady$.subscribe(isReady => {
        if (isReady) {
          this.currentUser = this.authService.getCurrentUser();
          this.loadUserProfile();
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  loadUserProfile(): void {
    this.httpClient.get(`${environment.microsoftGraph.baseUrl}/me`).subscribe({
      next: (profile) => {
        this.userProfile = profile;
      },
      error: (error) => {
        console.error('Error loading user profile:', error);
      }
    });
  }

  testGraphApiCall(): void {
    this.httpClient.get(`${environment.microsoftGraph.baseUrl}/me`).subscribe({
      next: (result) => {
        this.apiResult = {
          title: 'Microsoft Graph API Result',
          data: result
        };
      },
      error: (error) => {
        this.apiResult = {
          title: 'Microsoft Graph API Error',
          data: { error: 'API call failed', details: error.message }
        };
      }
    });
  }

  testCustomApiCall(): void {
    this.apiService.getWeatherForecast().subscribe({
      next: (result) => {
        this.apiResult = {
          title: 'Custom API Result',
          data: result
        };
      },
      error: (error) => {
        this.apiResult = {
          title: 'Custom API Error',
          data: { error: 'Custom API call failed', details: error.message }
        };
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}