using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CompleteWorkOrderCommand, WorkOrderDto?>
{
    public async Task<WorkOrderDto?> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await dbContext.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            return null;
        }

        workOrder.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);

        return await workOrder.ToDtoAsync(dbContext.Employees, cancellationToken);
    }
}