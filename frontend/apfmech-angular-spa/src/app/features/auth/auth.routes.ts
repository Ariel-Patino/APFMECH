import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  {
    path: 'auth/callback',
    loadComponent: () =>
      import('./callback/auth-callback.component').then(
        (module) => module.AuthCallbackComponent,
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((module) => module.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register/register.component').then(
        (module) => module.RegisterComponent,
      ),
  },
];