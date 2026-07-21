import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  host: { class: 'block min-h-dvh' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authService = inject(AuthService);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    rememberMe: [true],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    this.authService
      .login(this.form.getRawValue())
      .pipe(switchMap(() => this.authService.beginAuthorizationCodeFlow()))
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {},
        error: (err: HttpErrorResponse) => {
          if (err.status === 401) {
            this.errorMessage.set('The credentials provided are incorrect. Please check your email and password.');
          }
          this.form.controls.password.reset();
        }
      });
  }

  showError(controlName: 'email' | 'password', errorName: 'required' | 'email'): boolean {
    const control = this.form.controls[controlName];
    return control.touched && control.hasError(errorName);
  }
}