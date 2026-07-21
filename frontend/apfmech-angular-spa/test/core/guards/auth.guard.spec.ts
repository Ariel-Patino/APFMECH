import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { lastValueFrom, of, throwError } from 'rxjs';
import { AuthService } from '../../../src/app/core/services/auth.service';
import { authGuard } from '../../../src/app/core/guards/auth.guard';

describe('authGuard', () => {
  let authService: Pick<AuthService, 'me'> & {
    accessToken: jest.MockedFunction<() => string | null>;
    me: jest.MockedFunction<AuthService['me']>;
  };
  let router: Router;

  beforeEach(() => {
    authService = {
      accessToken: jest.fn().mockReturnValue('access-token-value'),
      me: jest.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authService,
        },
      ],
    });

    router = TestBed.inject(Router);
  });

  it('should allow activation when AuthService.me() succeeds', async () => {
    authService.me.mockReturnValue(
      of({
        userId: '3f8762a6-4b80-4955-8f65-8f2328f676f7',
        email: 'mechanic1@apfmech.local',
        fullName: 'Mechanic One',
        roles: ['Mechanic'],
        employeeId: 'f62d58bf-a6d3-48e9-9ed2-2f0d0cc63f7f',
        hasEmployeeProfile: true,
      }),
    );

    const result = await TestBed.runInInjectionContext(() =>
      lastValueFrom(authGuard({} as never, {} as never)),
    );

    expect(result).toBe(true);
  });

  it('should redirect to /login when AuthService.me() returns 401', async () => {
    authService.me.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401 })),
    );

    const result = await TestBed.runInInjectionContext(() =>
      lastValueFrom(authGuard({} as never, {} as never)),
    );

    expect(result instanceof UrlTree).toBe(true);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login');
  });

  it('should redirect to /login immediately when there is no access token', async () => {
    authService.accessToken.mockReturnValue(null);

    const result = await TestBed.runInInjectionContext(() =>
      lastValueFrom(authGuard({} as never, {} as never)),
    );

    expect(result instanceof UrlTree).toBe(true);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login');
    expect(authService.me).not.toHaveBeenCalled();
  });
});