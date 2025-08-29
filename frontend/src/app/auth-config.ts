import { Configuration, LogLevel } from '@azure/msal-browser';
import { environment } from '../environments/environment';

export const msalConfig: Configuration = {
  auth: {
    clientId: environment.msalConfig.auth.clientId,
    authority: environment.msalConfig.auth.authority,
    redirectUri: environment.msalConfig.auth.redirectUri,
    postLogoutRedirectUri: environment.msalConfig.auth.postLogoutRedirectUri,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: LogLevel, message: string) => {
        console.log(message);
      },
      logLevel: LogLevel.Info,
      piiLoggingEnabled: false,
    },
  },
};

export const loginRequest = {
  scopes: ['User.Read'], // Initial login with Graph API scope
  domainHint: 'corzent.com',
  prompt: 'select_account',
};

export const protectedResourceMap = new Map<string, Array<string>>([
  [environment.microsoftGraph.baseUrl + '/me', environment.microsoftGraph.scopes],
  [environment.apiConfig.baseUrl.replace('/api', ''), [environment.apiConfig.scope]],
]);