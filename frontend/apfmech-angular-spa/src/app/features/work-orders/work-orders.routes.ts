import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const workOrdersRoutes: Routes = [
  {
   path: 'work-orders',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/work-order-list/work-order-list.component').then(
            (m) => m.WorkOrderListComponent,
          ),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./components/work-order-detail/work-order-detail.component').then(
            (m) => m.WorkOrderDetailComponent,
          ),
      },
    ],
  },
];