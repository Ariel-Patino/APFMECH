---
name: APFMech .NET 10 Clean Architecture Skill
description: Enforces strict Clean Architecture, DDD layer segregation, dependency inversion, and pluggable module design for the APFMech enterprise solution.
version: 1.0.0
---

# 01-architecture.skill.md

## Purpose

This skill governs the architectural boundaries, dependency directions, and the pluggable module layout of the **APFMech** solution. It ensures that every code generation or refactoring action respects the **Clean Architecture** principles combined with **Domain‑Driven Design** (DDD) tactics, creating a system that is maintainable, testable, and ready for future expansion.

All generated code, project references, and namespace declarations must strictly adhere to the rules defined herein.

---

## Layer Segregation Rules (Strictly Inward Dependencies)

The solution is organised into four distinct layers. **Dependencies must point strictly inward** – no outward or cross‑layer shortcuts are permitted. Each layer has a specific responsibility and allowed dependency set.

### 1. Domain Layer
- **Project naming**: `APFMech.Domain`
- **Responsibility**: Central enterprise business rules, core logic, and domain models.
- **Must contain**:
  - **Rich entities** – encapsulating behaviour and state. Entities must enforce invariants through methods.
  - **Value objects** – immutable, self‑validating types (e.g., `Email`, `Money`, `Address`).
  - **Domain events** – to capture significant state changes.
  - **Domain exceptions** – custom exceptions for business rule violations.
  - **Aggregate roots** – defining transaction boundaries.
- **Forbidden practices**:
  - Public setters on entity properties – state changes must occur via explicit methods.
  - Anemic domain models (properties with get/set and no behaviour).
  - References to any external library beyond the .NET base class library (BCL) and standard NuGet packages for utility (e.g., `System.Text.Json` for serialisation attributes, but **no** ORM, DI, or persistence concerns).
- **Dependencies**: **None**. This layer is entirely isolated and framework‑agnostic.

### 2. Application Layer
- **Project naming**: `APFMech.Application`
- **Responsibility**: Orchestrating use cases, defining input/output contracts, and coordinating domain objects to fulfil business requirements.
- **Must contain**:
  - **Commands, Queries, and Handlers** – using **MediatR** (or similar CQRS pattern) for all operations.
  - **DTOs** – data transfer objects for input and output, mapped from/to domain entities.
  - **Application interfaces** – repository contracts (`IRepository<T>`), service interfaces, and unit of work abstractions. These are **defined** here but implemented in Infrastructure.
  - **Validation logic** – using FluentValidation or similar, applied to commands/queries.
  - **Feature/module folders** (see Pluggable Architecture section below).
- **Dependencies**:
  - **Only** the `APFMech.Domain` project.
  - MediatR and its extensions.
  - A validation library (e.g., FluentValidation).
  - `System.Text.Json` or similar for serialisation if required, but **no** database or HTTP concerns.
- **Forbidden**: Direct references to `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, or any persistence/UI framework.

### 3. Infrastructure Layer
- **Project naming**: `APFMech.Infrastructure`
- **Responsibility**: Implementing the interfaces defined in the Application layer, handling data persistence, external service integrations, and cross‑cutting concerns like logging or caching.
- **Must contain**:
  - **EF Core DbContext** – with `DbSet<T>` for aggregate roots, and configurations using Fluent API.
  - **Repository implementations** – concrete classes that implement the repository interfaces from Application.
  - **SQLite** – as the primary relational database for the MVP (with migrations managed here).
  - **External adapters** – e.g., email clients, file storage, or third‑party API wrappers.
  - **Unit of Work** – scoped persistence context.
- **Dependencies**:
  - The `APFMech.Application` project.
  - Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, and Microsoft.EntityFrameworkCore.Design.
  - Any libraries required for external integrations (e.g., `HttpClient`, `MailKit`).
- **Forbidden**: Direct references to the WebAPI layer or presentation logic. Business logic must reside in the Application or Domain layer – Infrastructure is purely an implementation detail.

### 4. WebAPI Layer (Presentation)
- **Project naming**: `APFMech.WebAPI`
- **Responsibility**: Exposing HTTP endpoints, handling authentication/authorisation, request/response serialisation, and middleware configuration.
- **Must contain**:
  - **Controllers** – thin endpoints that delegate to MediatR handlers. Controllers should contain **no business logic** – only parameter validation (via model binding) and mapping to commands/queries.
  - **`Program.cs`** – application startup, service registration, and middleware pipeline.
  - **JWT authentication setup** – with appropriate `[Authorize]` attributes on endpoints.
  - **Global exception handling middleware** – that translates domain exceptions into appropriate HTTP responses.
  - **OpenAPI/Swagger** documentation for all public endpoints.
- **Dependencies**:
  - The `APFMech.Application` and `APFMech.Infrastructure` projects.
  - Microsoft.AspNetCore.*, System.IdentityModel.Tokens.Jwt, Swashbuckle, etc.
- **Forbidden**: Direct dependency on `APFMech.Domain` – communication with the domain must happen via the Application layer.

**Dependency direction summary**:
WebAPI → Infrastructure → Application → Domain
WebAPI → Application (directly for MediatR)


**No circular references** are allowed. Build failures must occur if these rules are violated.

---

## Pluggable Architecture for Expansion

The system is designed to support future feature modules (Inventory, Preventive Maintenance, etc.) without modifying existing core domains. To achieve this:

- **All business logic must be isolated by feature/module folders inside the Application layer.**
- Each module is represented as a top‑level folder under `APFMech.Application`, e.g.:
  - `CoreMechanic/` – asset management, work orders, mechanic assignments (MVP core).
  - `Inventory/` – stock management, part tracking, warehouse locations.
  - `Maintenance/` – preventive schedules, calibration records, service history.
- **Within each module folder**, place all related:
  - Commands, Queries, and Handlers.
  - DTOs.
  - Validators.
  - Interfaces specific to that module (e.g., `IInventoryRepository`).
- **Cross‑module communication** must happen **only** through well‑defined Application interfaces or domain events – never through direct references. Handlers in one module may depend on interfaces defined in another module’s folder, but the referencing module must declare a project‑level dependency via `Application`’s internal structure – however, **prefer** using domain events to avoid tight coupling.
- **Infrastructure** implementations for each module’s interfaces are placed in corresponding folders under `APFMech.Infrastructure` (e.g., `Infrastructure/Repositories/Inventory/`).
- **New modules** can be introduced by:
  1. Creating a new folder in Application.
  2. Defining its commands/queries and interfaces.
  3. Implementing the interfaces in Infrastructure.
  4. Registering the new dependencies in the DI container via extension methods.
- **No changes** to existing CoreMechanic folder or Domain aggregates should be required when adding a new module – the architecture is truly pluggable.

---

## Validation Standards

Fail‑early principles are enforced to maintain data integrity and prevent invalid states from propagating.

### Domain Validation (Inside Entities and Value Objects)
- **Validation must occur inside the entity’s constructor or factory method** – before the instance is instantiated.
- All input parameters (primitive or value objects) must be validated at the moment of creation.
- If validation fails, throw a **custom Domain Exception** (e.g., `InvalidMechanicLicenseException`, `AssetSerialNumberTooLongException`) that clearly indicates the violation.
- **Example**:
  ```csharp
  public class Mechanic : Entity<MechanicId>
  {
      public string LicenseNumber { get; private set; }

      private Mechanic() { } // For EF Core

      public Mechanic(MechanicId id, string licenseNumber) : base(id)
      {
          if (string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Length > 20)
              throw new DomainException("Mechanic license number must be between 1 and 20 characters.");
          LicenseNumber = licenseNumber;
      }
  }
  ```