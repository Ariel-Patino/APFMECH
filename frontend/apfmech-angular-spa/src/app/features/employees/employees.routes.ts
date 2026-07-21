import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const employeesRoutes: Routes = [
  {
    path: 'employees',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/employees-list/employees-list.component').then(
        (module) => module.EmployeesListComponent,
      ),
  },
];