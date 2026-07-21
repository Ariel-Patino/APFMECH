using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.AssignMechanic;

public class AssignMechanicCommandHandler(IApplicationDbContext dbContext)
: IRequestHandler<AssignMechanicCommand, WorkOrderDto?>
{
public async Task<WorkOrderDto?> Handle(AssignMechanicCommand request, CancellationToken cancellationToken)
{
var workOrder = await dbContext.WorkOrders.GetByIdAsync(request.WorkOrderId, cancellationToken);
    var mechanic = await dbContext.Employees.GetByIdAsync(request.MechanicId, cancellationToken);

    if (workOrder is null)
    {
        return null;
    }

    if (mechanic is null || !mechanic.IsActive)
    {
        return null;
    }

    var isMechanicRole = mechanic.Roles.Any(role => string.Equals(role.Name, "Mechanic", StringComparison.OrdinalIgnoreCase));
    if (!isMechanicRole)
    {
        return null;
    }

    // Execute the state transition and raise Domain Events defined on our WorkOrder entity
    workOrder.AssignMechanic(request.MechanicId);    
    await dbContext.SaveChangesAsync(cancellationToken);
    return await workOrder.ToDtoAsync(dbContext.Employees, cancellationToken);
}


}