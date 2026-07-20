namespace APFMech.Domain.Common;

/// <summary>
/// Base class for all Aggregate Roots and Entities.
/// Manages entity identity, auditing metadata, and in-memory domain events.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    /// <summary>
    /// Read-only collection of raised domain events waiting to be dispatched.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Register a new event on this aggregate root.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears raised domain events after EF Core successfully commits and dispatches them.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Helper method to record entity updates safely.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}