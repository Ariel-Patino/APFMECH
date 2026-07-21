import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../../src/app/core/services/auth.service';
import { RegisterComponent } from '../../../../src/app/features/auth/register/register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  let component: RegisterComponent;
  let authService: {
    register: jest.Mock;
  };

  beforeEach(async () => {
    authService = {
      register: jest.fn().mockReturnValue(
        of({
          userId: '3f8762a6-4b80-4955-8f65-8f2328f676f7',
          email: 'new.user@apfmech.local',
          fullName: 'New User',
          roles: ['Mechanic'],
          employeeId: 'f62d58bf-a6d3-48e9-9ed2-2f0d0cc63f7f',
          hasEmployeeProfile: true,
        }),
      ),
    };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require email and password fields', () => {
    component.form.controls.email.setValue('');
    component.form.controls.password.setValue('');

    component.onSubmit();

    expect(component.form.controls.email.hasError('required')).toBe(true);
    expect(component.form.controls.password.hasError('required')).toBe(true);
    expect(authService.register).not.toHaveBeenCalled();
  });

  it('should call AuthService.register on submit with valid form values', () => {
    component.form.setValue({
      firstName: 'New',
      lastName: 'User',
      email: 'new.user@apfmech.local',
      password: 'Admin123!',
    });
    fixture.detectChanges();

    const formElement = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    formElement.dispatchEvent(new Event('submit'));

    expect(authService.register).toHaveBeenCalledWith({
      firstName: 'New',
      lastName: 'User',
      email: 'new.user@apfmech.local',
      password: 'Admin123!',
    });
  });
});