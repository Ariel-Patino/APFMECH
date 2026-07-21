using System.Collections.Generic;
using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders;
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

    var mappedWorkOrders = await Task.WhenAll(
        workOrders.Select(workOrder => workOrder.ToDtoAsync(dbContext.Employees, cancellationToken)));

    return mappedWorkOrders;
}
}
