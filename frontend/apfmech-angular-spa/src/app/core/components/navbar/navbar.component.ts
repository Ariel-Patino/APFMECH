import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { catchError, filter, of, startWith, switchMap } from 'rxjs';
import { AuthMeResponse } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = signal<AuthMeResponse | null>(null);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        startWith(null),
        switchMap(() =>
          this.authService.accessToken()
            ? this.authService.me().pipe(catchError(() => of(null)))
            : of(null),
        ),
      )
      .subscribe((user) => {
        this.currentUser.set(user);
      });
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(catchError(() => of(null)))
      .subscribe(() => {
        void this.router.navigateByUrl('/login');
      });
  }
}