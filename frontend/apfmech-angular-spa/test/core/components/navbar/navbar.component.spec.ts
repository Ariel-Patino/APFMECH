import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavigationEnd, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../../src/app/core/services/auth.service';
import { NavbarComponent } from '../../../../src/app/core/components/navbar/navbar.component';

describe('NavbarComponent', () => {
  let fixture: ComponentFixture<NavbarComponent>;
  let component: NavbarComponent;
  let authService: {
    accessToken: jest.Mock;
    me: jest.Mock;
    logout: jest.Mock;
  };
  let router: Router;

  beforeEach(async () => {
    authService = {
      accessToken: jest.fn().mockReturnValue('access-token-value'),
      me: jest.fn().mockReturnValue(
        of({
          userId: '3f8762a6-4b80-4955-8f65-8f2328f676f7',
          email: 'mechanic1@apfmech.local',
          fullName: 'Mechanic One',
          roles: ['Mechanic'],
          employeeId: 'f62d58bf-a6d3-48e9-9ed2-2f0d0cc63f7f',
          hasEmployeeProfile: true,
        }),
      ),
      logout: jest.fn().mockReturnValue(of(null)),
    };

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('should show protected links when user is authenticated', () => {
    fixture.detectChanges();

    const workOrdersLink = fixture.nativeElement.querySelector('[data-testid="work-orders-link"]');
    const employeesLink = fixture.nativeElement.querySelector('[data-testid="employees-link"]');

    expect(workOrdersLink).toBeTruthy();
    expect(employeesLink).toBeTruthy();
  });

  it('should hide protected links when user is not authenticated', () => {
    authService.accessToken.mockReturnValue(null);
    authService.me.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401 })),
    );

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const workOrdersLink = fixture.nativeElement.querySelector('[data-testid="work-orders-link"]');
    const employeesLink = fixture.nativeElement.querySelector('[data-testid="employees-link"]');

    expect(workOrdersLink).toBeNull();
    expect(employeesLink).toBeNull();
  });

  it('should call logout and redirect to /login when user clicks logout', () => {
    const navigateSpy = jest
      .spyOn(router, 'navigateByUrl')
      .mockResolvedValue(true);

    fixture.detectChanges();

    const logoutButton = fixture.nativeElement.querySelector('[data-testid="logout-button"]') as HTMLButtonElement;
    logoutButton.click();

    expect(authService.logout).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});