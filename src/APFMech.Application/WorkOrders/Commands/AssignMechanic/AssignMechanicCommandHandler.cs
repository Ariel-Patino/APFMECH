using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.AssignMechanic;

public class AssignMechanicCommandHandler(IApplicationDbContext dbContext)
: IRequestHandler<AssignMechanicCommand, WorkOrderDto?>
{
public async Task<WorkOrderDto?> Handle(AssignMechanicCommand request, CancellationToken cancellationToken)
{
var workOrder = await dbContext.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken);

    if (workOrder is null)
    {
        return null;
    }

    // Execute the state transition and raise Domain Events defined on our WorkOrder entity
    workOrder.AssignMechanic(request.MechanicId);    
    await dbContext.SaveChangesAsync(cancellationToken);
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