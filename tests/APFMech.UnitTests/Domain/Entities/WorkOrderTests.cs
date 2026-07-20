using APFMech.Domain.Entities;
using Xunit;

namespace APFMech.UnitTests.Domain.Entities;

public class WorkOrderTests
{
    [Fact]
    public void Create_ShouldInitializeWithPendingStatusAndRaiseEvent()
    {
        // Arrange
        var description = "Test repair";
        // Act
        var workOrder = WorkOrder.Create(description);

        // Assert
        Assert.Equal(WorkOrderStatus.Pending, workOrder.Status);
        Assert.StartsWith("WO-", workOrder.TrackingNumber);
        Assert.Contains(workOrder.DomainEvents, e => e is WorkOrderCreatedEvent);
    }

    [Fact]
    public void AssignMechanic_ShouldUpdateStatusAndRaiseEvent()
    {
        // Arrange
        var workOrder = WorkOrder.Create("Repair");
        var mechanicId = Guid.NewGuid();

        // Act
        workOrder.AssignMechanic(mechanicId);

        // Assert
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
        Assert.Equal(mechanicId, workOrder.AssignedMechanicId);
        Assert.Contains(workOrder.DomainEvents, e => e is WorkOrderAssignedEvent);
    }

    [Fact]
    public void AssignMechanic_ShouldThrow_WhenWorkOrderIsAlreadyCompleted()
    {
        // Arrange
        var workOrder = WorkOrder.Create("Repair");
        workOrder.AssignMechanic(Guid.NewGuid());
        workOrder.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => workOrder.AssignMechanic(Guid.NewGuid()));
    }

    [Fact]
    public void Complete_ShouldSetCompletedDate_WhenInProgress()
    {
        // Arrange
        var workOrder = WorkOrder.Create("Repair");
        workOrder.AssignMechanic(Guid.NewGuid());

        // Act
        workOrder.Complete();

        // Assert
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.NotNull(workOrder.CompletedAtUtc);
        Assert.Contains(workOrder.DomainEvents, e => e is WorkOrderCompletedEvent);
    }
}