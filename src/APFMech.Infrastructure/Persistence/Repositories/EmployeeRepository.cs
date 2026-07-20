using APFMech.Application.Common.Interfaces;
using APFMech.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APFMech.Infrastructure.Persistence.Repositories;

public class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .Include(x => x.Roles)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await dbContext.Employees.AddAsync(employee, cancellationToken);
    }

    public void Update(Employee employee)
    {
        dbContext.Employees.Update(employee);
    }
}