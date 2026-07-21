import { Routes } from '@angular/router';
import { NotFoundComponent } from './shared/components/not-found/not-found.component';

export const routes: Routes = [
	{
		path: '',
		pathMatch: 'full',
		redirectTo: 'work-orders',
	},
	{
		path: '',
		loadChildren: () =>
			import('./features/auth/auth.routes').then((module) => module.authRoutes),
	},
	{
		path: '',
		loadChildren: () =>
			import('./features/work-orders/work-orders.routes').then(
				(module) => module.workOrdersRoutes,
			),
	},
	{
		path: '',
		loadChildren: () =>
			import('./features/employees/employees.routes').then(
				(module) => module.employeesRoutes,
			),
	},
	{
		path: '**',
		component: NotFoundComponent,
	},
];
