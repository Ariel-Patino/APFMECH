import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { AuthService } from '../../../src/app/core/services/auth.service';
import {
  AuthMeResponse,
  LoginRequest,
  RegisterUserRequest,
} from '../../../src/app/core/models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), AuthService],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST /api/auth/login', () => {
    const payload: LoginRequest = {
      email: 'mechanic1@apfmech.local',
      password: 'Admin123!',
      rememberMe: true,
    };

    service.login(payload).subscribe((response) => {
      expect(response).toBeNull();
    });

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(null);
  });

  it('should POST /api/auth/register', () => {
    const payload: RegisterUserRequest = {
      email: 'new.user@apfmech.local',
      password: 'Admin123!',
      firstName: 'New',
      lastName: 'User',
    };

    const expected: AuthMeResponse = {
      userId: '3f8762a6-4b80-4955-8f65-8f2328f676f7',
      email: payload.email,
      fullName: 'New User',
      roles: ['Mechanic'],
      employeeId: 'f62d58bf-a6d3-48e9-9ed2-2f0d0cc63f7f',
      hasEmployeeProfile: true,
    };

    service.register(payload).subscribe((response) => {
      expect(response).toEqual(expected);
    });

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(expected);
  });

  it('should POST /api/auth/logout', () => {
    service.logout().subscribe((response) => {
      expect(response).toBeNull();
    });

    const req = httpMock.expectOne('/api/auth/logout');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush(null);
  });

  it('should GET /api/auth/me', () => {
    const expected: AuthMeResponse = {
      userId: '3f8762a6-4b80-4955-8f65-8f2328f676f7',
      email: 'mechanic1@apfmech.local',
      fullName: 'Mechanic One',
      roles: ['Mechanic'],
      employeeId: 'f62d58bf-a6d3-48e9-9ed2-2f0d0cc63f7f',
      hasEmployeeProfile: true,
    };

    service.me().subscribe((response) => {
      expect(response).toEqual(expected);
    });

    const req = httpMock.expectOne('/api/auth/me');
    expect(req.request.method).toBe('GET');
    req.flush(expected);
  });
});