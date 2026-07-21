import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../../../src/app/core/services/auth.service';
import { authInterceptor } from '../../../src/app/core/interceptors/auth.interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        AuthService,
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add withCredentials and Authorization header for /api/ requests when a token exists', () => {
    authService.setAccessToken('access-token-value');

    httpClient.get('/api/workorders').subscribe();

    const req = httpMock.expectOne('/api/workorders');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.headers.get('Authorization')).toBe(
      'Bearer access-token-value',
    );
    req.flush([]);
  });

  it('should add withCredentials without Authorization header for /api/ requests when token is missing', () => {
    authService.clearAccessToken();

    httpClient.get('/api/workorders').subscribe();

    const req = httpMock.expectOne('/api/workorders');
    expect(req.request.withCredentials).toBe(true);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });

  it('should leave non-api requests unchanged', () => {
    authService.setAccessToken('access-token-value');

    httpClient.get('https://example.com/assets/config.json').subscribe();

    const req = httpMock.expectOne('https://example.com/assets/config.json');
    expect(req.request.withCredentials).toBe(false);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });
});