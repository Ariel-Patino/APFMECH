# APFMech

APFMech is a full-stack workshop management system for automotive repair operations. It is designed to manage work orders, mechanics, employees, authentication, and the operational flow of a modern garage.

The solution follows Clean Architecture and a test-first mindset inspired by TDD. Business rules live in the domain and application layers, while infrastructure and presentation remain replaceable.

## Overview

APFMech helps a workshop team to:

- create and track work orders
- assign mechanics to pending jobs
- complete work orders with validation enforced by the backend
- manage employees and their activity status
- authenticate users through an OpenID Connect flow
- keep the frontend as a thin SPA over a REST API

The repository contains two main applications:

- Backend: ASP.NET Core Web API in .NET 10
- Frontend: Angular 21 single-page application

## Technologies Used

### Backend

- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- OpenIddict for OpenID Connect / OAuth-style authentication
- ASP.NET Identity
- FluentValidation
- MediatR
- Serilog
- SQLite in the current checked-in configuration
- xUnit
- NSubstitute

### Frontend

- Angular 21
- TypeScript
- RxJS
- Jest

### Notes on the database

The codebase is EF Core-based and database-provider agnostic, but the current development configuration uses SQLite through `Data Source=apfmech.db`. If you want to run the solution on SQL Server, adjust the Infrastructure persistence configuration and the development connection string accordingly.

## Architecture

APFMech is organized into four layers:

### 1. Domain

Contains the core business model:

- entities
- value objects and enums
- domain events
- invariants and lifecycle rules

This layer has no dependency on the application, infrastructure, or UI layers.

### 2. Application

Contains use cases and orchestration:

- commands and queries
- handlers
- validators
- DTOs
- application interfaces

This layer coordinates domain entities and repository abstractions.

### 3. Infrastructure

Contains technical implementations:

- EF Core persistence
- repositories
- Identity integration
- OpenIddict configuration
- data seeding
- logging setup

### 4. Presentation

Contains the external interfaces:

- ASP.NET Core Web API controllers
- middleware
- Angular SPA

### Key principles used

- Dependency Inversion: the application depends on abstractions, not concrete persistence or identity implementations.
- Repository Pattern: data access is isolated behind repository contracts.
- DTO Pattern: the API and frontend exchange stable, purpose-specific data contracts.

## Project Structure

```text
APFMech/
├── src/
│   ├── APFMech.Domain/
│   ├── APFMech.Application/
│   ├── APFMech.Infrastructure/
│   └── APFMech.WebAPI/
└── tests/
    └── APFMech.UnitTests/
```

The Angular SPA is located in `frontend/apfmech-angular-spa`.

## Prerequisites

Before running the solution, make sure you have installed:

- .NET 10.0 SDK
- Node.js 16+ 
- npm 8+
- a local database engine if you plan to switch from SQLite to SQL Server

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd APFMech
```

### 2. Configure the backend

Open `src/APFMech.WebAPI/appsettings.Development.json` and review the persistence settings:

```json
{
  "Persistence": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=apfmech.db"
  }
}
```

If you are deploying with SQL Server, update the provider and connection string to match your environment.

### 3. Build the backend

```bash
dotnet build
```

### 4. Run the backend

```bash
dotnet run --launch-profile https --project ./src/APFMech.WebAPI/APFMech.WebAPI.csproj
```

Backend URLs in development:

- HTTPS: `https://localhost:7123`
- HTTP: `http://localhost:5034`

### 5. Run the frontend

```bash
cd frontend/apfmech-angular-spa
npm install
npm run start
```

Frontend URL:

- `http://localhost:4200`

The frontend is configured to proxy API and OpenID Connect calls to the backend.

## Authentication

APFMech uses an OpenID Connect-based login flow backed by ASP.NET Identity and OpenIddict.

The main flow works like this:

1. The user submits credentials to the backend login endpoint.
2. The backend establishes the authenticated session.
3. The SPA starts the OpenID Connect authorization code flow with PKCE.
4. The backend issues an access token and refresh token.
5. The Angular app stores the access token in memory and uses it for authenticated API calls.

### Test credentials

Use the following seeded development account:

| Email | Password | Role |
| --- | --- | --- |
| admin@apfmech.local | Admin123! | Admin |

Important:

- These credentials are only available in Development.
- The seed process does not run in Production.
- If you delete the database file, the seed will be recreated the next time the app starts in Development.

## Data Seeding

The backend seeds development data only when `ASPNETCORE_ENVIRONMENT=Development`.

Current seeded data includes:

- roles
- development admin user
- sample employees
- sample work orders
- OpenID Connect clients and scopes required by the SPA

If you need a clean local reset, delete the SQLite database file at:

```text
src/APFMech.WebAPI/apfmech.db
```

Then run the backend again in Development.

## API Documentation

### Authentication endpoints

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/auth/register` | Creates a new user account |
| POST | `/api/auth/login` | Validates credentials and starts the authenticated session |
| POST | `/api/auth/logout` | Ends the current session |
| GET | `/api/auth/me` | Returns the current authenticated user profile |
| GET | `/connect/authorize` | Starts the OpenID Connect authorization flow |
| POST | `/connect/token` | Exchanges an authorization code or refresh token for access tokens |
| GET | `/connect/userinfo` | Returns OIDC user info claims |

### Work Orders endpoints

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/workorders` | Creates a new work order |
| GET | `/api/workorders` | Returns all work orders |
| GET | `/api/workorders/{id}` | Returns a work order by id |
| PUT | `/api/workorders/{id}/assign` | Assigns a mechanic to a work order |
| PUT | `/api/workorders/{id}/complete` | Marks a work order as completed |

### Employee endpoints

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/employees` | Returns all employees |
| PATCH | `/api/employees/{id}/disable` | Disables an employee |
| DELETE | `/api/employees/{id}` | Permanently deletes an employee and its identity user |

### Login example

Request:

```bash
curl -X POST "https://localhost:7123/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@apfmech.local",
    "password": "Admin123!",
    "rememberMe": true
  }'
```

Response:

```http
HTTP/1.1 204 No Content
```

After that, the SPA completes the OpenID Connect flow by calling `/connect/authorize` and then `/connect/token`.

### Work order creation example

Request:

```bash
curl -X POST "https://localhost:7123/api/workorders" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <access-token>" \
  -d '{
    "description": "Replace hydraulic seal on Fleet Truck #102"
  }'
```

Response:

```json
{
  "id": "4f2fb4f2-80e2-4ddb-9e88-b9a456103d12",
  "trackingNumber": "WO-20260720-ABC123",
  "description": "Replace hydraulic seal on Fleet Truck #102",
  "status": "Pending",
  "assignedMechanicId": null,
  "assignedMechanicFullName": null,
  "createdAtUtc": "2026-07-20T08:30:00Z"
}
```

## Testing

APFMech is designed with TDD in mind.

### Backend tests

```bash
dotnet test .\tests\APFMech.UnitTests\APFMech.UnitTests.csproj
```

The backend unit tests currently cover:

- Domain entities
- Application commands and queries
- validators
- DTO mapping logic

### Frontend tests

```bash
cd frontend/apfmech-angular-spa
npm test -- --runInBand
```

### Testing strategy

A practical TDD flow for this repository is:

1. write a failing test for the domain rule or application use case
2. implement the smallest possible change
3. refactor safely
4. rerun the test suite

## Future Enhancements

1. **Mejora del sistema de roles**: pasar de un simple campo Role a un sistema granular de permisos, con asignación dinámica de roles y dashboards específicos por rol, incluyendo logging de auditoría.
2. **Immutabilidad de Work Orders**: hacer que las órdenes de trabajo se vuelvan inmutables después de ser firmadas digitalmente por cliente y mecánico. Implementar workflow de solicitud de cambios con versionado histórico.
3. **Sistema avanzado de tareas**: subtareas, dependencias entre tareas, registro de horas con start/stop, notificaciones automáticas y flujos de aprobación para decisiones del cliente.
4. **Funcionalidades adicionales**: notificaciones en tiempo real con SignalR, portal de cliente, integración con inventario, dashboards y reportes, PWA responsive, soporte multi-tenant, attachments y notificaciones por email.

## Troubleshooting

### Backend does not start

- Confirm that the .NET 10 SDK is installed.
- Make sure no other process is using the selected port.
- Verify the launch profile:

```bash
dotnet run --launch-profile https --project ./src/APFMech.WebAPI/APFMech.WebAPI.csproj
```

- If the local database is corrupted, delete `src/APFMech.WebAPI/apfmech.db` and start again in Development.

### Frontend cannot connect to the API

- Confirm the backend is running on `https://localhost:7123`.
- Confirm the frontend is started from `frontend/apfmech-angular-spa`.
- Recheck the proxy configuration in `frontend/apfmech-angular-spa/proxy.conf.json`.

### Authentication errors

- Use the development credentials shown above.
- Make sure the backend is running in Development so the seed data is created.
- Clear browser storage if a stale token is causing issues.
- Verify that the callback URL is exactly `http://localhost:4200/auth/callback`.

### Database reset

If you want to start from zero locally, delete the SQLite file and rerun the backend in Development:

```text
src/APFMech.WebAPI/apfmech.db
```

## License

MIT.

## Support

For support, contact the APFMech development team or the repository owner responsible for this system.

## Frontend Notes

The Angular app includes its own local README for SPA-specific notes, but the root README is the main project documentation.
