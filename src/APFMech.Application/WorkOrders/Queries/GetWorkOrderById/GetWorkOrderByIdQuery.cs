using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Queries.GetWorkOrderById;

public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDto?>;

public class GetWorkOrderByIdQueryHandler(IApplicationDbContext dbContext)
: IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto?>
{
public async Task<WorkOrderDto?> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
{
var workOrder = await dbContext.WorkOrders.GetByIdAsync(request.Id, cancellationToken);

    if (workOrder is null)
    {
        return null;
    }

    return new WorkOrderDto(
        workOrder.Id,
        workOrder.TrackingNumber,
        workOrder.Description,
        workOrder.Status.ToString(),
        workOrder.AssignedMechanicId,
        workOrder.CreatedAtUtc
    );
}


}