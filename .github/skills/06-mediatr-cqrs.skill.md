---
name: APFMech CQRS with MediatR Skill
description: Enforces Command Query Responsibility Segregation (CQRS) using MediatR, with strict separation of reads/writes, feature‑based organization, and pipeline behaviours for cross‑cutting concerns.
version: 1.0.0
---

# 06-mediatr-cqrs.skill.md

## Purpose

This skill governs the separation of read and write concerns and enforces strict task decoupling across the **Application layer** of the APFMech solution. It establishes the non‑negotiable rules for:

- Defining **Commands** (mutations) and **Queries** (reads) as immutable records.
- Implementing **Handlers** that are single‑responsibility and thin.
- Organising code by **feature** (folder‑by‑feature) to avoid monolithic controller/service anti‑patterns.
- Applying **MediatR Pipeline Behaviors** for cross‑cutting concerns like validation, logging, and performance metrics.

All generated CQRS artifacts must comply with these standards to ensure maintainability, testability, and a clear separation of concerns.

---

## CQRS Architecture Rules

### 1. Commands – Structural Mutations

- **Purpose**: Represent write operations that change system state (Create, Update, Delete, or any action that modifies domain aggregates).
- **Definition**:
  - Must be **immutable** – use C# `public record` types.
  - Must implement `ICommand<TResult>` or `IRequest<TResult>` (from MediatR).
  - The result type `TResult` should be a **clear, predictable** response (e.g., `Guid` for created ID, `Unit` for void‑like commands, or a dedicated result DTO).
- **Naming**: Use imperative verbs, e.g., `CreateWorkOrderCommand`, `UpdateMechanicCommand`, `DeleteAssetCommand`.
- **Validation**: Apply `FluentValidation` validators to each command (see Pipeline Behaviors).
- **Return**: Commands should return the ID of the created/updated resource when applicable, or `Unit` if no meaningful result.
- **Forbidden**:
  - Returning domain entities directly – use DTOs or IDs.
  - Side‑effects inside the command (it’s a data container only).

**Example**:
```csharp
public record CreateWorkOrderCommand(
    string Title,
    string Description,
    Guid MechanicId,
    DateTime? ScheduledDate
) : IRequest<Guid>;
```
### 2. Queries – Side‑Effect‑Free Reads
- **Purpose:** Retrieve data without modifying state. Must be idempotent and pure.
- **Definition:**
    - Immutable public record implementing IQuery<TResult> or IRequest<TResult>.
    - The result type should be tailored to the frontend/client needs (DTOs), not domain entities.
- **Naming:** Use descriptive names, e.g., GetWorkOrderByIdQuery, GetAllMechanicsQuery, GetPendingTasksQuery.
- **Parameters:** Include filtering, sorting, pagination parameters as needed (use dedicated DTOs for complex queries).
- **Forbidden:**
    - Returning domain entities – use Projection or AutoMapper to map to DTOs.
    - Any side‑effect (e.g., logging is allowed but not mutation).
**Example:**
```csharp
public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDto>;
```
### 3. Handlers – Single Responsibility
- **One handler per command or query** – do not handle multiple request types in a single handler.
- **Keep handlers thin:**
    - Delegate persistence to repositories (defined in Application, implemented in Infrastructure).
    - Delegate domain logic to domain entities or domain services.
    - Use IMapper (AutoMapper) or manual mapping to transform entities to DTOs.
- **Dependencies:**
    - Repositories (IWorkOrderRepository, etc.).
    - Unit of Work (or DbContext wrapper) for transaction coordination.
    - Optional services (e.g., IEmailService, ICurrentUserService).
- **Asynchronous:** All handlers must be async (Task<TResult>).
- **Exception handling:** Let domain exceptions bubble up; pipeline behaviors and global middleware will handle them.

**Example Handler:**
```csharp
public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = new WorkOrder(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.MechanicId,
            request.ScheduledDate
        );
        _repository.Add(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return workOrder.Id;
    }
}
```
---

## Folder‑by‑Feature Organization

**Forbidden:** Dumping all commands, queries, and handlers into a single folder like Application/Commands/ or Application/Queries/.
**Mandated structure:** Group by business feature under the Application/Features/ folder.
Example layout:
```text
Application/
├── Features/
│   ├── WorkOrders/
│   │   ├── Commands/
│   │   │   ├── CreateWorkOrder/
│   │   │   │   ├── CreateWorkOrderCommand.cs
│   │   │   │   ├── CreateWorkOrderCommandHandler.cs
│   │   │   │   └── CreateWorkOrderCommandValidator.cs
│   │   │   ├── UpdateWorkOrder/
│   │   │   │   ├── UpdateWorkOrderCommand.cs
│   │   │   │   ├── UpdateWorkOrderCommandHandler.cs
│   │   │   │   └── UpdateWorkOrderCommandValidator.cs
│   │   │   └── DeleteWorkOrder/
│   │   │       └── ...
│   │   ├── Queries/
│   │   │   ├── GetWorkOrderById/
│   │   │   │   ├── GetWorkOrderByIdQuery.cs
│   │   │   │   ├── GetWorkOrderByIdQueryHandler.cs
│   │   │   │   └── (optionally, a validator if needed)
│   │   │   └── GetAllWorkOrders/
│   │   │       └── ...
│   │   ├── DTOs/
│   │   │   ├── WorkOrderDto.cs
│   │   │   └── (other DTOs)
│   │   └── Mappings/
│   │       └── WorkOrderProfile.cs (AutoMapper profile)
│   ├── Mechanics/
│   │   └── (similar structure)
│   └── Inventory/
│       └── (similar structure)
├── Common/
│   ├── Interfaces/           # Shared application interfaces
│   ├── Behaviors/            # MediatR Pipeline behaviors (Validation, Logging, etc.)
│   └── Exceptions/           # Custom application exceptions
└── ...
```
Rationale: This structure isolates feature changes, reduces merge conflicts, and enables pluggable module insertion without affecting existing features.

---

## MediatR Pipeline Behaviors

Pipeline behaviors are executed around every request handler, enabling cross‑cutting concerns without 
polluting handlers.

### 1. Validation Behavior (Mandatory)

- **Purpose:** Automatically validate all commands and queries before they reach the handler.
- **Implementation:**
    - Use FluentValidation to define validators for each request.
    - Register a generic ValidationBehavior<TRequest, TResponse> that implements IPipelineBehavior<TRequest, TResponse>.
    - The behavior scans for validators (IValidator<TRequest>) and runs them.
    - If any validation fails, throw a ValidationException (or a custom APFValidationException) – this is then caught by middleware and returned as a 400 Bad Request with structured Problem Details.
- **Ensure** that the behavior is registered in the DI container before any other behaviors (order matters).
- **For commands** – validate all inputs (e.g., required fields, lengths, formats).
- **For queries** – validate any filter/sort parameters, but not necessary for simple ID queries.
**Example:**
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }
        return await next();
    }
}
```
### 2. Additional Behaviors (Recommended)
- **LoggingBehavior** – log request/response with timing (for performance monitoring).
- **PerformanceBehavior** – measure execution time and warn if threshold exceeded.
- **TransactionBehavior** – wrap handler in a database transaction (if not using unit of work already).
- **ExceptionHandlingBehavior** – optional, but recommended to catch unhandled exceptions and wrap them in a specific application exception – though global middleware is also acceptable.

### 3. Registration
In Program.cs (or a dedicated service registration):
```csharp
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateWorkOrderCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    // Add other behaviors in order (validation first, then logging, etc.)
});
```
---

All CQRS and MediatR implementations must follow these standards. Any deviation, such as mixing command and query logic or skipping validation, must be explicitly justified and approved.
