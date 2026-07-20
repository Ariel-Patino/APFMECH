using APFMech.Domain.Common;
using MediatR;

namespace APFMech.Application.Common.Events;

public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}