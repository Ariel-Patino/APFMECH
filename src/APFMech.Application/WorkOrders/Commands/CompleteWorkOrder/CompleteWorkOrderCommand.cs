using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.CompleteWorkOrder;

public record CompleteWorkOrderCommand(Guid WorkOrderId) : IRequest<WorkOrderDto?>;