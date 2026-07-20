using APFMech.Domain.Common;

namespace APFMech.Domain.Entities;

public record WorkOrderCreatedEvent(Guid WorkOrderId, string TrackingNumber) : DomainEvent;
public record WorkOrderAssignedEvent(Guid WorkOrderId, Guid MechanicId) : DomainEvent;
public record WorkOrderCompletedEvent(Guid WorkOrderId, DateTime CompletedAtUtc) : DomainEvent;