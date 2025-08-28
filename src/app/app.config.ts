import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MSAL_INSTANCE, MSAL_GUARD_CONFIG, MSAL_INTERCEPTOR_CONFIG, MsalService, MsalGuard, MsalBroadcastService, MsalInterceptor } from '@azure/msal-angular';
import { IPublicClientApplication, PublicClientApplication, InteractionType } from '@azure/msal-browser';
import { routes } from './app.routes';
import { msalConfig, protectedResourceMap, loginRequest } from './auth-config';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication(msalConfig);
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideNoopAnimations(),
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useValue: {
        interactionType: InteractionType.Redirect,
        authRequest: loginRequest
      }
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useValue: {
        interactionType: InteractionType.Redirect,
        protectedResourceMap
      }
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService
  ]
};
