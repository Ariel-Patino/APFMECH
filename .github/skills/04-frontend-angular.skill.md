---
name: APFMech Angular Frontend Skill
description: Enforces enterprise‑grade Angular architecture, reactive state management with Signals, responsive UI design, and strict browser console hygiene for the APFMech frontend application.
version: 1.0.0
---

# 04-frontend-angular.skill.md

## Purpose

This skill governs the directory structure, component lifecycle, reactive state management, and performance standards of the **Angular** frontend interface in the **APFMech** project. It ensures that all generated frontend code:

- Follows modern Angular best practices (standalone components, Signals, lazy loading).
- Is maintainable, testable, and scalable across multiple enterprise modules.
- Delivers a responsive, mobile‑first user experience.
- Maintains a clean, error‑free browser console in development and production.

All AI‑generated TypeScript, HTML, and SCSS/CSS must adhere to these rules without exception.

---

## Angular Architecture & Project Layout

### 1. Standalone Component Paradigm

- **All components, directives, and pipes must be standalone** (`standalone: true`).
- **Legacy `NgModule` classes are forbidden** for feature or shared modules. Use standalone components with `imports` arrays to declare dependencies.
- Root `AppComponent` and bootstrap configuration in `main.ts` must use `bootstrapApplication` (no `AppModule`).

### 2. Feature‑Based Directory Structure

The Angular project must be organised into clear functional domains, promoting separation of concerns and future module pluggability.
```text
src/
├── app/
│   ├── core/                      # Application‑wide singletons and utilities
│   │   ├── services/              # Global services (Auth, API client, ErrorHandler)
│   │   ├── guards/                # Route guards (functional)
│   │   ├── interceptors/          # HTTP interceptors (token attachment, logging)
│   │   ├── models/                # TypeScript interfaces/enums shared across features
│   │   └── constants/             # Application‑wide constants (API endpoints, roles)
│   │
│   ├── shared/                    # Reusable, presentation‑only artifacts
│   │   ├── components/            # Dumb/stateless components (buttons, modals, cards)
│   │   ├── directives/            # Custom attribute directives
│   │   └── pipes/                 # Pure pipes (date formatting, currency, etc.)
│   │
│   ├── features/                  # Top‑level business modules (lazy‑loaded)
│   │   ├── work-orders/           # Asset work order management
│   │   │   ├── components/        # Feature‑specific components
│   │   │   ├── containers/        # Smart components (stateful, data‑fetching)
│   │   │   ├── services/          # Feature‑specific services (state, API facades)
│   │   │   ├── models/            # Feature‑specific interfaces
│   │   │   ├── store/             # Signal‑based state stores (if applicable)
│   │   │   └── work-orders.routes.ts # Lazy route definition
│   │   ├── inventory/             # Inventory management module
│   │   ├── maintenance/           # Preventive maintenance module
│   │   └── auth/                  # Authentication (login, callback, logout)
│   │
│   └── app.routes.ts              # Root route configuration (uses loadChildren)
│
├── assets/                        # Static assets (images, fonts, translations)
├── environments/                  # Environment configuration (dev, prod)
└── styles/                        # Global styles (Tailwind or custom)
```


### 3. Lazy Loading for Feature Modules

- **Every feature under `features/` must be lazy‑loaded** using Angular’s `loadChildren` in the root route configuration.
- **Route definitions** must be placed in a `*.routes.ts` file adjacent to the feature folder.
- **Preloading strategy**: Use `PreloadAllModules` only for modules that are critical after initial load; otherwise, rely on the default lazy loading.
- **Route guards** (`canActivate`, `canLoad`) must be applied at the feature route level to protect entire modules.

---

## Reactive State Management & Components

### 1. Angular Signals for Localised State

- **Prefer Angular Signals (`signal`, `computed`, `effect`) over `BehaviorSubject`** for component‑local state and simple shared state services.
- **Signal usage**:
  - `signal()` – for mutable, reactive state.
  - `computed()` – for derived state that depends on other signals.
  - `effect()` – for side effects that react to state changes (e.g., logging, persisting to `localStorage`).
- **Avoid** unnecessary RxJS subscriptions (`subscribe()`) for UI data binding; use `async` pipe and `toObservable()` only when bridging with RxJS libraries.

**Example – Signal‑based component state**:
```typescript
@Component({
  selector: 'app-work-order-list',
  standalone: true,
  template: `
    <div *ngFor="let item of workOrders()">
      {{ item.title }}
    </div>
    <button (click)="addWorkOrder()">Add</button>
  `
})
export class WorkOrderListComponent {
  private workOrderService = inject(WorkOrderService);
  workOrders = signal<WorkOrder[]>([]);

  constructor() {
    effect(() => {
      // Auto‑fetch when signal changes (or initial load)
      this.workOrderService.fetchAll().subscribe(orders => this.workOrders.set(orders));
    });
  }

  addWorkOrder() {
    // Update signal directly; the effect will not re‑trigger unless dependency changes.
    this.workOrders.update(list => [...list, { id: 0, title: 'New Order' }]);
  }
}
```
### 2. Smart/Presentational (Container/Component) Pattern
- **Smart components (containers) – responsible for:**
    - Fetching data from services.
    - Managing application state (signals, RxJS streams).
    - Handling user events and dispatching them to services or stores.
    - Located in features/<module>/containers/.
- **Presentational components (dumb) – responsible for:**
    - Rendering UI based on @Input() data.
    - Emitting events via @Output() (or custom event emitters).
    - No direct dependencies on services or state management.
    - Located in features/<module>/components/ or shared/components/.
- **Testing:** Presentational components can be tested purely with inputs/outputs; smart components are tested with mocked services.

### 3. Responsive Design (Mobile‑First)
- **Framework:** Use TailwindCSS (preferred) or Angular Material’s layout tokens. No custom media‑query chaos.
- **Mobile‑first approach:**
    - Design for smallest screen first (sm: prefix for small screens, md:, lg:, xl: for larger).
    - Use flexible layout primitives: flexbox/grid, percentage‑based widths, min‑h‑screen, and max‑w‑full.
- **Responsive breakpoints (consistent with Tailwind):**
    - sm: 640px (mobile)
    - md: 768px (tablet)
    - lg: 1024px (desktop)
    - xl: 1280px (large desktop) 
    - 2xl: 1536px (extra large)
- **Accessibility:** Ensure contrast ratios, focus indicators, and ARIA labels are correctly applied.

---
## Console Hygiene & Error Prevention
### 1. Zero Warnings/Errors in Browser Console
- **All generated code must compile and run without emitting any:**
    - JavaScript errors (TypeError, ReferenceError, etc.).
    - Angular runtime errors (e.g., ExpressionChangedAfterItHasBeenChecked).
    - Deprecation warnings (e.g., use of legacy APIs).
- ** Strict mode:** Enable "strict": true in tsconfig.json to enforce type safety and catch null/undefined errors at compile time.
### 2. Proper Async Handling and Unsubscription
- **For RxJS subscriptions** that are not automatically completed:
    - Use takeUntilDestroyed (from @angular/core/rxjs-interop) for component‑scoped subscriptions.
    - Alternatively, use the async pipe in templates, which automatically handles unsubscription.
- **If using** subscribe() in a service or component (rare), ensure the subscription is stored and cleaned up in ngOnDestroy or using takeUntil with a destroy subject.
- **Example using** takeUntilDestroyed:
```typescript
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

constructor() {
  this.workOrderService.getUpdates()
    .pipe(takeUntilDestroyed(this))
    .subscribe(update => this.handleUpdate(update));
}
```
- **For** effect(): Avoid long‑running side‑effects; if they involve observables, use toObservable and manage the subscription inside the effect with a cleanup mechanism.
### 3. Global ErrorHandler Implementation
- **Create a custom** ErrorHandler that traps all unhandled client‑side errors, logging them to a monitoring service (e.g., Sentry, Application Insights) and displaying a user‑friendly fallback UI.
- **Must be registered** in main.ts or app.config.ts using { provide: ErrorHandler, useClass: GlobalErrorHandler }.
- **Rules for the handler:**
    - Never swallow errors without logging.
    - In production, show a generic message (e.g., "An unexpected error occurred. Please try again.").
    - In development, forward the full error to the console for debugging.
    - The handler must not throw additional exceptions.
- **Example implementation:**
```typescript
@Injectable()
export class GlobalErrorHandler extends ErrorHandler {
  override handleError(error: any): void {
    // Log to console in dev
    console.error('Global error caught:', error);
    // Send to monitoring service
    this.logError(error);
    // Display user feedback (via a toast/notification service)
    this.notificationService.error('Something went wrong. Our team has been notified.');
    // Do not re‑throw
  }
}
```
### 4. Development vs. Production Hygiene
- **Development:**
    - Use Angular’s development mode (default) to get maximum warnings and hints.
    - Enable source maps for easier debugging.
- **Production:**
    - Enable production mode (enableProdMode()) to disable framework‑level checks and improve performance.
    - Minify bundles and remove debug logs using build‑time optimisations (e.g., Terser with drop_console: true).

---

Compliance with these frontend standards is mandatory. Any generated Angular code that introduces console warnings, errors, or legacy patterns must be corrected before a pull request is considered ready for review.