import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) {
    return next(request);
  }

  const accessToken = inject(AuthService).accessToken();
  if (!accessToken) {
    return next(
      request.clone({
        withCredentials: true,
      }),
    );
  }

  return next(
    request.clone({
      withCredentials: true,
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
      },
    }),
  );
};