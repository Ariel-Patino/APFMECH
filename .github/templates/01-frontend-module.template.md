---
name: APFMech Angular Feature Module Scaffold Template
description: A reproducible markdown blueprint for scaffolding a complete standalone Angular feature module using Signals, lazy-loading, and mobile-first responsive design.
version: 1.0.0
---

# 01-frontend-module.template.md

## Template Metadata

- **Feature Name**: `<FeatureName>` (e.g., `WorkOrders`, `Inventory`, `Maintenance`)
- **Feature Route Segment**: `/<feature-name>` (e.g., `/work-orders`, `/inventory`)
- **Module Type**: Standalone Components (no NgModules)
- **State Management**: Angular Signals (`signal`, `computed`, `effect`)
- **Styling**: Tailwind CSS (mobile-first utility classes)
- **Lazy Loading**: Yes – routed via `loadChildren` in the root configuration

---

## Directory Structure Blueprint

Place all files under the `src/app/features/<feature-name>/` directory. Use **kebab-case** for folder and file names (except for TypeScript classes which use PascalCase).
src/app/features/<feature-name>/
├── pages/
│   └── <feature-name>-list/                     # Smart/Container component
│       ├── <feature-name>-list.component.ts
│       ├── <feature-name>-list.component.html
│       └── <feature-name>-list.component.scss    # (optional)
├── components/
│   └── <feature-name>-card/                     # Presentational component
│       ├── <feature-name>-card.component.ts
│       ├── <feature-name>-card.component.html
│       └── <feature-name>-card.component.scss    # (optional)
├── services/
│   ├── <feature-name>.service.ts                # API client (HttpClient)
│   └── <feature-name>.facade.ts                 # State facade (Signals)
├── models/
│   └── <feature-name>.model.ts                  # TypeScript interfaces/types
├── constants/
│   └── <feature-name>.constants.ts              # Feature-specific constants 
└── <feature-name>.routes.ts                    # Lazy route definitions


> **Note**: For larger features, additional subfolders under `pages/` (e.g., `-detail`, `-edit`) and `components/` can be added as needed.

---

## Boilerplate Code Blocks

Replace all `<...>` placeholders with your actual names, types, and business logic.

### 1. Feature Model (TypeScript Interface)

**Path**: `src/app/features/<feature-name>/models/<feature-name>.model.ts`

```typescript
export interface <FeatureName> {
  id: string;                     // or number, depending on backend
  name: string;
  description: string;
  createdAt: Date;
  // Add other properties as needed
}
```
### 2. API Service (HttpClient)
**Path**: `src/app/features/<feature-name>/services/<feature-name>.service.ts`
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { <FeatureName> } from '../models/<feature-name>.model';

@Injectable({ providedIn: 'root' })
export class <FeatureName>Service {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/<feature-name>'; // Adjust to actual API endpoint

  getItems(): Observable<<FeatureName>[]> {
    return this.http.get<<FeatureName>[]>(this.apiUrl);
  }

  getItemById(id: string): Observable<<FeatureName>> {
    return this.http.get<<FeatureName>>(`${this.apiUrl}/${id}`);
  }

  createItem(item: Omit<<FeatureName>, 'id' | 'createdAt'>): Observable<<FeatureName>> {
    return this.http.post<<FeatureName>>(this.apiUrl, item);
  }

  updateItem(id: string, item: Partial<<FeatureName>>): Observable<<FeatureName>> {
    return this.http.put<<FeatureName>>(`${this.apiUrl}/${id}`, item);
  }

  deleteItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```
### 3. State Facade with Signals
**Path**: `src/app/features/<feature-name>/services/<feature-name>.facade.ts`
```typescript
import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { <FeatureName>Service } from './<feature-name>.service';
import { <FeatureName> } from '../models/<feature-name>.model';

@Injectable({ providedIn: 'root' })
export class <FeatureName>Facade {
  private readonly service = inject(<FeatureName>Service);

  // State signals (private mutable)
  private readonly itemsSubject = signal<<FeatureName>[]>([]);
  private readonly loadingSubject = signal<boolean>(false);
  private readonly errorSubject = signal<string | null>(null);
  private readonly selectedIdSubject = signal<string | null>(null);

  // Public readonly signals
  readonly items = this.itemsSubject.asReadonly();
  readonly loading = this.loadingSubject.asReadonly();
  readonly error = this.errorSubject.asReadonly();

  // Computed: derived state
  readonly selectedItem = computed(() => {
    const id = this.selectedIdSubject();
    return id ? this.itemsSubject().find(item => item.id === id) ?? null : null;
  });

  readonly totalCount = computed(() => this.itemsSubject().length);

  // Load all items
  async loadItems(): Promise<void> {
    this.loadingSubject.set(true);
    this.errorSubject.set(null);
    try {
      const items = await this.service.getItems().toPromise();
      this.itemsSubject.set(items);
    } catch (err) {
      this.errorSubject.set('Failed to load items. Please try again.');
      console.error('[<FeatureName>Facade] loadItems error:', err);
    } finally {
      this.loadingSubject.set(false);
    }
  }

  // Select an item
  selectItem(id: string): void {
    this.selectedIdSubject.set(id);
  }

  // Clear selection
  clearSelection(): void {
    this.selectedIdSubject.set(null);
  }

  // Create a new item
  async createItem(item: Omit<<FeatureName>, 'id' | 'createdAt'>): Promise<void> {
    this.loadingSubject.set(true);
    this.errorSubject.set(null);
    try {
      const newItem = await this.service.createItem(item).toPromise();
      this.itemsSubject.update(current => [...current, newItem]);
    } catch (err) {
      this.errorSubject.set('Failed to create item.');
      throw err;
    } finally {
      this.loadingSubject.set(false);
    }
  }

  // Update an existing item
  async updateItem(id: string, updates: Partial<<FeatureName>>): Promise<void> {
    this.loadingSubject.set(true);
    this.errorSubject.set(null);
    try {
      const updated = await this.service.updateItem(id, updates).toPromise();
      this.itemsSubject.update(current =>
        current.map(item => item.id === id ? updated : item)
      );
      if (this.selectedIdSubject() === id) {
        this.selectedIdSubject.set(id); // Trigger computed refresh
      }
    } catch (err) {
      this.errorSubject.set('Failed to update item.');
      throw err;
    } finally {
      this.loadingSubject.set(false);
    }
  }

  // Delete an item
  async deleteItem(id: string): Promise<void> {
    this.loadingSubject.set(true);
    this.errorSubject.set(null);
    try {
      await this.service.deleteItem(id).toPromise();
      this.itemsSubject.update(current => current.filter(item => item.id !== id));
      if (this.selectedIdSubject() === id) {
        this.selectedIdSubject.set(null);
      }
    } catch (err) {
      this.errorSubject.set('Failed to delete item.');
      throw err;
    } finally {
      this.loadingSubject.set(false);
    }
  }
}
```
### 4. Presentational Component: Item Card
**Path**: `src/app/features/<feature-name>/components/<feature-name>-card/<feature-name>-card.component.ts`
```typescript
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { <FeatureName> } from '../../models/<feature-name>.model';

@Component({
  selector: 'app-<feature-name>-card',
  standalone: true,
  imports: [], // Add any necessary imports
  template: `
    <div class="p-4 border rounded-lg shadow hover:shadow-md transition-shadow cursor-pointer"
         (click)="select.emit(item().id)">
      <h3 class="text-lg font-semibold truncate">{{ item().name }}</h3>
      <p class="text-sm text-gray-600 line-clamp-2">{{ item().description }}</p>
      <div class="mt-2 flex justify-between items-center">
        <span class="text-xs text-gray-400">
          {{ item().createdAt | date:'mediumDate' }}
        </span>
        <button class="text-red-500 hover:text-red-700"
                (click)="delete.emit(item().id); $event.stopPropagation()">
          <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      </div>
    </div>
  `,
  styles: `
    :host {
      display: block;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class <FeatureName>CardComponent {
  readonly item = input.required<<FeatureName>>();
  readonly select = output<string>();
  readonly delete = output<string>();
}
```
### 5. Smart/Container Component: List Page
**Path**: `src/app/features/<feature-name>/pages/<feature-name>-list/<feature-name>-list.component.ts`
```typescript
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AsyncPipe, DatePipe } from '@angular/common';
import { <FeatureName>Facade } from '../../services/<feature-name>.facade';
import { <FeatureName>CardComponent } from '../../components/<feature-name>-card/<feature-name>-card.component';

@Component({
  selector: 'app-<feature-name>-list',
  standalone: true,
  imports: [
    DatePipe,
    AsyncPipe,
    RouterLink,
    <FeatureName>CardComponent
  ],
  template: `
    <div class="container mx-auto px-4 py-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
        <h1 class="text-2xl font-bold text-gray-800">
          <FeatureName>s
          <span class="text-sm font-normal text-gray-500 ml-2">
            ({{ facade.totalCount() }} total)
          </span>
        </h1>
        <button class="btn btn-primary" [routerLink]="['./new']">
          + New <FeatureName>
        </button>
      </div>

      <!-- Loading State -->
      @if (facade.loading()) {
        <div class="flex justify-center py-8">
          <span class="loading loading-spinner loading-lg"></span>
        </div>
      }

      <!-- Error State -->
      @if (facade.error()) {
        <div class="alert alert-error mb-4">
          {{ facade.error() }}
          <button class="btn btn-sm btn-ghost" (click)="facade.loadItems()">Retry</button>
        </div>
      }

      <!-- Empty State -->
      @if (!facade.loading() && !facade.error() && facade.items().length === 0) {
        <div class="text-center py-12">
          <p class="text-gray-500">No items found. Create your first one!</p>
        </div>
      }

      <!-- Items Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        @for (item of facade.items(); track item.id) {
          <app-<feature-name>-card
            [item]="item"
            (select)="onSelect($event)"
            (delete)="onDelete($event)"
          />
        }
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class <FeatureName>ListComponent implements OnInit {
  private readonly facade = inject(<FeatureName>Facade);

  ngOnInit(): void {
    this.facade.loadItems();
  }

  onSelect(id: string): void {
    this.facade.selectItem(id);
    // Optionally navigate to detail page
    // this.router.navigate(['/feature-name', id]);
  }

  onDelete(id: string): void {
    if (confirm('Are you sure you want to delete this item?')) {
      this.facade.deleteItem(id);
    }
  }
}
```
### 6. Lazy Route Definitions
**Path**: `src/app/features/<feature-name>/<feature-name>.routes.ts`
```typescript
import { Route } from '@angular/router';
import { <FeatureName>ListComponent } from './pages/<feature-name>-list/<feature-name>-list.component';

export const <FEATURE_NAME>_ROUTES: Route[] = [
  {
    path: '',
    component: <FeatureName>ListComponent,
    // Add guards if needed, e.g.:
    // canActivate: [AuthGuard, RoleGuard('Admin')]
  },
  // Additional routes (detail, create, edit) can be added here:
  // {
  //   path: 'new',
  //   loadComponent: () => import('./pages/<feature-name>-create/<feature-name>-create.component')
  //                          .then(m => m.<FeatureName>CreateComponent)
  // },
  // {
  //   path: ':id',
  //   loadComponent: () => import('./pages/<feature-name>-detail/<feature-name>-detail.component')
  //                          .then(m => m.<FeatureName>DetailComponent)
  // }
];
```
---
## Integration Checklist
Follow these steps to integrate the new feature module into the root routing configuration:
### 1. Add the route to app.routes.ts:
- In src/app/app.routes.ts, add a new route entry for the feature using loadChildren:
```typescript
{
  path: '<feature-name>',
  loadChildren: () => import('./features/<feature-name>/<feature-name>.routes')
                        .then(m => m.<FEATURE_NAME>_ROUTES)
}
```
- Ensure the route path uses kebab-case (e.g., work-orders, inventory).
### 2. Update navigation:
- If the feature should be accessible from the main navigation, add a link in the AppComponent or a shared navigation component:
```html
<a routerLink="/<feature-name>" routerLinkActive="active">
  <FeatureName>
</a>
```
### 3. Register the service (if not providedIn: 'root'):
- All services in the template use providedIn: 'root', so no additional registration is needed.
- If you decide to use non-root providers, add them to app.config.ts.
### 4. Add environment-specific configuration (optional):
- If the feature uses environment variables (e.g., API base URL), ensure they are defined in environments/*.ts.
### 5. Test the route:
- Run the application and navigate to http://localhost:4200/<feature-name>.
- Verify that the lazy-loaded bundle loads and the component renders correctly.

---

*After filling in all placeholders, this template should produce a fully functional, standalone, and responsive Angular feature module that adheres to APFMech frontend standards.*