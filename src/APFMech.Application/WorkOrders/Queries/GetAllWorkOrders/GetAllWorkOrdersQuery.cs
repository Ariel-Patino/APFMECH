using System.Collections.Generic;
using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Queries.GetAllWorkOrders;

public record GetAllWorkOrdersQuery : IRequest<IReadOnlyList<WorkOrderDto>>;

public class GetAllWorkOrdersQueryHandler(IApplicationDbContext dbContext)
: IRequestHandler<GetAllWorkOrdersQuery, IReadOnlyList<WorkOrderDto>>
{
public async Task<IReadOnlyList<WorkOrderDto>> Handle(GetAllWorkOrdersQuery request, CancellationToken cancellationToken)
{
var workOrders = await dbContext.WorkOrders.GetAllAsync(cancellationToken);

    return workOrders
        .Select(workOrder => new WorkOrderDto(
            workOrder.Id,
            workOrder.TrackingNumber,
            workOrder.Description,
            workOrder.Status.ToString(),
            workOrder.AssignedMechanicId,
            workOrder.CreatedAtUtc
        ))
        .ToList();
}
}
