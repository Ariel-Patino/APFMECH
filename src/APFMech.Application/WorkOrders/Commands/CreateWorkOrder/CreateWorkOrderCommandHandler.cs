using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders;
using APFMech.Domain.Entities;
using MediatR;

namespace APFMech.Application.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandler(IApplicationDbContext dbContext) 
    : IRequestHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    public async Task<WorkOrderDto> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Create domain aggregate (triggers domain invariants and raises WorkOrderCreatedEvent)
        var workOrder = WorkOrder.Create(request.Description);

        // 2. Persist aggregate via application context contract
        await dbContext.WorkOrders.AddAsync(workOrder, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 3. Explicit manual mapping to DTO (No AutoMapper)
        return await workOrder.ToDtoAsync(dbContext.Employees, cancellationToken);
    }
}