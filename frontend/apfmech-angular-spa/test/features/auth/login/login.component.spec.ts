import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../../src/app/core/services/auth.service';
import { LoginComponent } from '../../../../src/app/features/auth/login/login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let authService: {
    login: jest.Mock;
    beginAuthorizationCodeFlow: jest.Mock;
  };

  beforeEach(async () => {
    authService = {
      login: jest.fn().mockReturnValue(of(null)),
      beginAuthorizationCodeFlow: jest.fn().mockReturnValue(of(void 0)),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require email and password fields', () => {
    component.form.controls.email.setValue('');
    component.form.controls.password.setValue('');

    component.onSubmit();

    expect(component.form.controls.email.hasError('required')).toBe(true);
    expect(component.form.controls.password.hasError('required')).toBe(true);
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should call AuthService.login on submit with valid form values', () => {
    component.form.setValue({
      email: 'mechanic1@apfmech.local',
      password: 'Admin123!',
      rememberMe: true,
    });
    fixture.detectChanges();

    const formElement = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    formElement.dispatchEvent(new Event('submit'));

    expect(authService.login).toHaveBeenCalledWith({
      email: 'mechanic1@apfmech.local',
      password: 'Admin123!',
      rememberMe: true,
    });
    expect(authService.beginAuthorizationCodeFlow).toHaveBeenCalled();
  });
});