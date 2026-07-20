using APFMech.Application.Common.Interfaces;
using APFMech.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APFMech.Infrastructure.Persistence.Repositories;

public class WorkOrderRepository(ApplicationDbContext dbContext) : IWorkOrderRepository
{
    public async Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkOrders.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        await dbContext.WorkOrders.AddAsync(workOrder, cancellationToken);
    }

    public void Update(WorkOrder workOrder)
    {
        dbContext.WorkOrders.Update(workOrder);
    }
}