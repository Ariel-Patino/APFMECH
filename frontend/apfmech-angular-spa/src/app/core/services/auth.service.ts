import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, from, map, switchMap, tap, throwError } from 'rxjs';
import {
  AuthMeResponse,
  LoginRequest,
  OidcTokenResponse,
  RegisterUserRequest,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly authApiBaseUrl = '/api/auth';
  private readonly oidcAuthorizeEndpoint = '/connect/authorize';
  private readonly oidcTokenEndpoint = '/connect/token';
  private readonly oidcClientId = 'apfmech-angular-spa';
  private readonly oidcScopes = 'openid profile email roles apfmech_api';

  private readonly codeVerifierStorageKey = 'apfmech.pkce.code_verifier';
  private readonly stateStorageKey = 'apfmech.pkce.state';

  private accessTokenValue: string | null = null;
  private refreshTokenValue: string | null = null;

  constructor(private readonly httpClient: HttpClient) {}

  accessToken(): string | null {
    return this.accessTokenValue;
  }

  setAccessToken(accessToken: string): void {
    this.accessTokenValue = accessToken;
  }

  refreshToken(): string | null {
    return this.refreshTokenValue;
  }

  clearAccessToken(): void {
    this.accessTokenValue = null;
    this.refreshTokenValue = null;
  }

  login(request: LoginRequest): Observable<null> {
    return this.httpClient.post<null>(`${this.authApiBaseUrl}/login`, request);
  }

  register(request: RegisterUserRequest): Observable<AuthMeResponse> {
    return this.httpClient.post<AuthMeResponse>(
      `${this.authApiBaseUrl}/register`,
      request,
    );
  }

  logout(): Observable<null> {
    return this.httpClient
      .post<null>(`${this.authApiBaseUrl}/logout`, {})
      .pipe(tap(() => this.clearAccessToken()));
  }

  me(): Observable<AuthMeResponse> {
    return this.httpClient.get<AuthMeResponse>(`${this.authApiBaseUrl}/me`);
  }

  beginAuthorizationCodeFlow(): Observable<void> {
    return from(this.buildAuthorizationRequest()).pipe(
      tap((authorizationUrl) => this.redirectToAuthorization(authorizationUrl)),
      map(() => void 0),
    );
  }

  completeAuthorizationCodeFlow(search: string): Observable<void> {
    const params = this.parseQueryString(search);
    const code = params.get('code');
    const state = params.get('state');
    const storedState = sessionStorage.getItem(this.stateStorageKey);
    const codeVerifier = sessionStorage.getItem(this.codeVerifierStorageKey);

    if (!code || !state || !storedState || state !== storedState || !codeVerifier) {
      this.clearPkceSession();
      return throwError(() => new Error('Invalid authorization response.'));
    }

    const redirectUri = this.buildRedirectUri();
    const body = new HttpParams({
      fromObject: {
        grant_type: 'authorization_code',
        client_id: this.oidcClientId,
        code,
        redirect_uri: redirectUri,
        code_verifier: codeVerifier,
      },
    }).toString();

    return this.httpClient
      .post<OidcTokenResponse>(this.oidcTokenEndpoint, body, {
        headers: new HttpHeaders({
          'Content-Type': 'application/x-www-form-urlencoded',
        }),
      })
      .pipe(
        tap((tokenResponse) => {
          this.accessTokenValue = tokenResponse.access_token;
          this.refreshTokenValue = tokenResponse.refresh_token ?? null;
          this.clearPkceSession();
        }),
        map(() => void 0),
      );
  }

  private async buildAuthorizationRequest(): Promise<string> {
    const codeVerifier = this.generateSecureRandom(64);
    const state = this.generateSecureRandom(32);
    const nonce = this.generateSecureRandom(32);
    const codeChallenge = await this.createCodeChallenge(codeVerifier);

    sessionStorage.setItem(this.codeVerifierStorageKey, codeVerifier);
    sessionStorage.setItem(this.stateStorageKey, state);

    const queryParams = new URLSearchParams({
      response_type: 'code',
      client_id: this.oidcClientId,
      redirect_uri: this.buildRedirectUri(),
      scope: this.oidcScopes,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state,
      nonce,
    });

    return `${this.oidcAuthorizeEndpoint}?${queryParams.toString()}`;
  }

  private redirectToAuthorization(url: string): void {
    window.location.assign(url);
  }

  private buildRedirectUri(): string {
    return `${window.location.origin}/auth/callback`;
  }

  private parseQueryString(search: string): URLSearchParams {
    return new URLSearchParams(search.startsWith('?') ? search.slice(1) : search);
  }

  private clearPkceSession(): void {
    sessionStorage.removeItem(this.codeVerifierStorageKey);
    sessionStorage.removeItem(this.stateStorageKey);
  }

  private generateSecureRandom(length: number): string {
    const allowedCharacters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    const randomValues = new Uint8Array(length);
    crypto.getRandomValues(randomValues);

    return Array.from(randomValues, (value) =>
      allowedCharacters[value % allowedCharacters.length],
    ).join('');
  }

  private async createCodeChallenge(codeVerifier: string): Promise<string> {
    const data = new TextEncoder().encode(codeVerifier);
    const digest = await crypto.subtle.digest('SHA-256', data);
    return this.base64UrlEncode(new Uint8Array(digest));
  }

  private base64UrlEncode(bytes: Uint8Array): string {
    const binary = String.fromCharCode(...bytes);
    return btoa(binary)
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=+$/g, '');
  }
}