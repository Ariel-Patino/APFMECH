using APFMech.Application.Common.Events;
using APFMech.Application.Common.Interfaces;
using APFMech.Domain.Common;
using APFMech.Domain.Entities;
using APFMech.Infrastructure.Identity;
using APFMech.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore;

namespace APFMech.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) 
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
     private WorkOrderRepository? _workOrderRepository;
    private EmployeeRepository? _employeeRepository;
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Employee> Employees => Set<Employee>();
    IWorkOrderRepository IApplicationDbContext.WorkOrders => 
        _workOrderRepository ??= new WorkOrderRepository(this);
    IEmployeeRepository IApplicationDbContext.Employees =>
        _employeeRepository ??= new EmployeeRepository(this);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent)
                ?? throw new InvalidOperationException($"Could not create notification for domain event type {domainEvent.GetType().Name}.");

            await publisher.Publish(notification, cancellationToken);
        }
    }
}