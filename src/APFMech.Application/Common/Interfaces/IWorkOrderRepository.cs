using APFMech.Domain.Entities;

namespace APFMech.Application.Common.Interfaces;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default);
    void Update(WorkOrder workOrder);
}