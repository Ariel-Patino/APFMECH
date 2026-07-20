using MediatR;

namespace APFMech.Application.WorkOrders.Commands.CreateWorkOrder;

public record WorkOrderDto(
    Guid Id,
    string TrackingNumber,
    string Description,
    string Status,
    Guid? AssignedMechanicId,
    DateTime CreatedAtUtc
);

public record CreateWorkOrderCommand(string Description) : IRequest<WorkOrderDto>;