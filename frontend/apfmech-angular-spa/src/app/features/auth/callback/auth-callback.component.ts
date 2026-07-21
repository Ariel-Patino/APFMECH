import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auth-callback.component.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthCallbackComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.authService.completeAuthorizationCodeFlow(window.location.search).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigateByUrl('/work-orders');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Authorization failed. Please sign in again.');
      },
    });
  }

  goToLogin(): void {
    void this.router.navigateByUrl('/login');
  }
}