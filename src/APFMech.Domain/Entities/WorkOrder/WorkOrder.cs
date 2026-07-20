using APFMech.Domain.Common;

namespace APFMech.Domain.Entities;

/// <summary>
/// Rich Domain Aggregate Root representing a mechanic maintenance assignment.
/// </summary>
public class WorkOrder : BaseEntity
{
    // C# required empty constructor for EF Core materialization
    private WorkOrder() { }

    public string TrackingNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Pending;
    public Guid? AssignedMechanicId { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Named Factory pattern to instantiate a valid WorkOrder with business invariants.
    /// </summary>
    public static WorkOrder Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TrackingNumber = $"WO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Description = description.Trim(),
            Status = WorkOrderStatus.Pending
        };

        workOrder.RaiseDomainEvent(new WorkOrderCreatedEvent(workOrder.Id, workOrder.TrackingNumber));
        return workOrder;
    }

    /// <summary>
    /// Assigns a mechanic to handle the work order.
    /// </summary>
    public void AssignMechanic(Guid mechanicId)
    {
        if (Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign a mechanic to a finalized work order.");

        AssignedMechanicId = mechanicId;
        Status = WorkOrderStatus.InProgress;
        MarkAsUpdated();

        RaiseDomainEvent(new WorkOrderAssignedEvent(Id, mechanicId));
    }

    /// <summary>
    /// Transition the work order into a finalized state.
    /// </summary>
    public void Complete()
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Work order must be In Progress to be completed.");
        
        if (AssignedMechanicId == null)
            throw new InvalidOperationException("Cannot complete an unassigned work order.");

        Status = WorkOrderStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        MarkAsUpdated();

        RaiseDomainEvent(new WorkOrderCompletedEvent(Id, CompletedAtUtc.Value));
    }
}