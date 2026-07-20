using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.AssignMechanic;

public record AssignMechanicCommand(Guid WorkOrderId, Guid MechanicId) : IRequest<WorkOrderDto?>;