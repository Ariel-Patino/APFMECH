---
name: APFMech Backend Module Scaffold Template
description: A reproducible markdown blueprint for scaffolding a new pluggable feature module across the .NET 10 Clean Architecture layers using MediatR and CQRS.
version: 1.0.0
---

# 00-backend-module.template.md

## Template Metadata

- **Feature Name**: `<FeatureName>` (e.g., `Inventory`, `Maintenance`, `WorkOrder`)
- **Aggregate Root**: `<AggregateRootEntity>` (e.g., `InventoryItem`, `MaintenanceSchedule`, `WorkOrder`)
- **Primary Key Type**: `<IdType>` (e.g., `Guid`, `int`, `long`)
- **Module Owner**: `<Team/Individual>`
- **Pluggable Module**: Yes – this module must be introduced without modifying existing core modules.

---

## Directory Structure Blueprint

The following file tree must be created under the respective layers. All files are to be placed in the exact locations shown.

APFMech.Domain/
└── Entities/
    └── <FeatureName>.cs                          # Rich domain entity (aggregate root)

APFMech.Application/
└── Features/
    └── <FeatureName>s/                           # Pluralised feature folder
        ├── Commands/
        │   └── Create<FeatureName>/
        │       ├── Create<FeatureName>Command.cs
        │       ├── Create<FeatureName>CommandValidator.cs
        │       └── Create<FeatureName>CommandHandler.cs
        ├── Queries/
        │   └── Get<FeatureName>ById/
        │       ├── Get<FeatureName>ByIdQuery.cs
        │       ├── Get<FeatureName>ByIdQueryHandler.cs
        │       └── <FeatureName>Dto.cs
        ├── DTOs/                                  # Additional DTOs (if needed)
        │   └── <FeatureName>ListDto.cs
        └── Mappings/
            └── <FeatureName>Profile.cs            # AutoMapper profile

APFMech.Infrastructure/
└── Persistence/
    ├── Configurations/
    │   └── <FeatureName>Configuration.cs         # EF Core Fluent mapping
    └── Repositories/
        └── <FeatureName>Repository.cs            # Repository implementation


> **Note**: For simpler modules, you may omit the separate repository implementation if you use a generic repository pattern.

---

## Boilerplate Code Blocks

Copy the following code templates and replace all `<...>` placeholders with your actual names, types, and business logic.

### 1. Rich Domain Entity

**Path**: `APFMech.Domain/Entities/<FeatureName>.cs`

```csharp
using APFMech.Domain.SeedWork;

namespace APFMech.Domain.Entities;

public class <FeatureName> : AggregateRoot<<IdType>>
{
    // Private constructor for EF Core
    private <FeatureName>() { }

    // Business properties – private setters
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Add other properties as needed

    // Factory method for creation with validation
    public static <FeatureName> Create(
        string name,
        string description,
        // additional parameters
    )
    {
        // Business rule validations
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new DomainException($"{nameof(name)} must be between 1 and 100 characters.");
        // Add other rules

        return new <FeatureName>
        {
            Id = <IdType>.NewGuid(),   // or other ID generation
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business behaviour methods
    public void UpdateDetails(string newName, string newDescription)
    {
        // Validate and update
        if (string.IsNullOrWhiteSpace(newName) || newName.Length > 100)
            throw new DomainException("Name must be between 1 and 100 characters.");
        Name = newName.Trim();
        Description = newDescription?.Trim();
    }
}
```
### 2. MediatR Command
**Path**: `APFMech.Application/Features/<FeatureName>s/Commands/Create<FeatureName>/Create<FeatureName>Command.cs`
```csharp
using MediatR;

namespace APFMech.Application.Features.<FeatureName>s.Commands.Create<FeatureName>;

public record Create<FeatureName>Command(
    string Name,
    string Description,
    // additional parameters
    // Use 'required' for mandatory fields if using C# 14
) : IRequest<<IdType>>;
```
### 3. FluentValidation Validator
**Path**: `APFMech.Application/Features/<FeatureName>s/Commands/Create<FeatureName>/Create<FeatureName>CommandValidator.cs`
```csharp
using FluentValidation;

namespace APFMech.Application.Features.<FeatureName>s.Commands.Create<FeatureName>;

public class Create<FeatureName>CommandValidator : AbstractValidator<Create<FeatureName>Command>
{
    public Create<FeatureName>CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters.");

        // Add additional rules
    }
}
```
### 4. MediatR Command Handler
**Path**: `APFMech.Application/Features/<FeatureName>s/Commands/Create<FeatureName>/Create<FeatureName>CommandHandler.cs`
```csharp
using MediatR;
using APFMech.Application.Common.Interfaces;
using APFMech.Domain.Entities;

namespace APFMech.Application.Features.<FeatureName>s.Commands.Create<FeatureName>;

public class Create<FeatureName>CommandHandler : IRequestHandler<Create<FeatureName>Command, <IdType>>
{
    private readonly I<FeatureName>Repository _repository;   // or IRepository<FeatureName>
    private readonly IUnitOfWork _unitOfWork;

    public Create<FeatureName>CommandHandler(
        I<FeatureName>Repository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<<IdType>> Handle(Create<FeatureName>Command request, CancellationToken cancellationToken)
    {
        // Validate uniqueness if needed – use repository or domain service

        var entity = <FeatureName>.Create(
            name: request.Name,
            description: request.Description
            // additional parameters
        );

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```
### 5. Query and DTO
**Path**: `APFMech.Application/Features/<FeatureName>s/Queries/Get<FeatureName>ById/Get<FeatureName>ByIdQuery.cs`
```csharp
using MediatR;
using APFMech.Application.Features.<FeatureName>s.DTOs;

namespace APFMech.Application.Features.<FeatureName>s.Queries.Get<FeatureName>ById;

public record Get<FeatureName>ByIdQuery(<IdType> Id) : IRequest<<FeatureName>Dto>;
```
**Path**: `APFMech.Application/Features/<FeatureName>s/DTOs/<FeatureName>Dto.cs`
```csharp
namespace APFMech.Application.Features.<FeatureName>s.DTOs;

public record <FeatureName>Dto(
    <IdType> Id,
    string Name,
    string Description,
    DateTime CreatedAt
    // other fields
);
```
**Path**: `APFMech.Application/Features/<FeatureName>s/Queries/Get<FeatureName>ById/Get<FeatureName>ByIdQueryHandler.cs`
```csharp
using MediatR;
using APFMech.Application.Common.Interfaces;
using APFMech.Application.Features.<FeatureName>s.DTOs;
using AutoMapper;

namespace APFMech.Application.Features.<FeatureName>s.Queries.Get<FeatureName>ById;

public class Get<FeatureName>ByIdQueryHandler : IRequestHandler<Get<FeatureName>ByIdQuery, <FeatureName>Dto>
{
    private readonly I<FeatureName>Repository _repository;
    private readonly IMapper _mapper;

    public Get<FeatureName>ByIdQueryHandler(I<FeatureName>Repository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<<FeatureName>Dto> Handle(Get<FeatureName>ByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException($"<FeatureName> with id {request.Id} not found.");

        return _mapper.Map<<FeatureName>Dto>(entity);
    }
}
```
### 6. EF Core Configuration
**Path**: `APFMech.Infrastructure/Persistence/Configurations/<FeatureName>Configuration.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using APFMech.Domain.Entities;

namespace APFMech.Infrastructure.Persistence.Configurations;

public class <FeatureName>Configuration : IEntityTypeConfiguration<<FeatureName>>
{
    public void Configure(EntityTypeBuilder<<FeatureName>> builder)
    {
        builder.ToTable("<FeatureNames>");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Add indexes for frequently queried fields
        builder.HasIndex(e => e.Name);
    }
}
```
### 7. AutoMapper Profile
**PATH**: `APFMech.Application/Features/<FeatureName>s/Mappings/<FeatureName>Profile.cs`
```csharp
using AutoMapper;
using APFMech.Application.Features.<FeatureName>s.DTOs;
using APFMech.Domain.Entities;

namespace APFMech.Application.Features.<FeatureName>s.Mappings;

public class <FeatureName>Profile : Profile
{
    public <FeatureName>Profile()
    {
        CreateMap<<FeatureName>, <FeatureName>Dto>();
        // Add other mappings as needed
    }
}
```
---
## Integration Instructions
- **Register the new DbSet** in the AppDbContext (in Infrastructure) by adding:
```csharp
    public DbSet<<FeatureName>> <FeatureNames> { get; set; }
```
- **Add the configuration** to the OnModelCreating override:
```csharp
    modelBuilder.ApplyConfiguration(new <FeatureName>Configuration());
```
- **Add repository interface** to the Application layer (if not using a generic repository) – e.g., I<FeatureName>Repository.
- **Implement repository** in Infrastructure (if custom queries are needed).
- **Register dependencies** in the DI container:
    - services.AddScoped<I<FeatureName>Repository, <FeatureName>Repository>();
    - Ensure MediatR and AutoMapper assemblies are scanned.
- **Create a database migration** using the dotnet ef migrations add Add<FeatureName>Module.
- **Add API endpoints** in the WebAPI layer – e.g., a controller with [Authorize] attributes and endpoints that delegate to MediatR handlers.

---

*After filling in all placeholders, this template should produce a fully integrated, pluggable module that adheres to APFMech architectural standards.*