namespace APFMech.Domain.Common;

/// <summary>
/// Contract marker for all domain events in the system.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
