import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.accessToken()) {
    return of(router.createUrlTree(['/login']));
  }

  return authService.me().pipe(
    map(() => true),
    catchError((error: unknown) => {
      return of(router.createUrlTree(['/login']));
    }),
  );
};