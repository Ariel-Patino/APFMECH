namespace APFMech.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IWorkOrderRepository WorkOrders { get; }
    IEmployeeRepository Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}