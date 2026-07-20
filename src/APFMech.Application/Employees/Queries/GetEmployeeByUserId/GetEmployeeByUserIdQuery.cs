using APFMech.Application.Common.Interfaces;
using MediatR;

namespace APFMech.Application.Employees.Queries.GetEmployeeByUserId;

public record GetEmployeeByUserIdQuery(Guid UserId) : IRequest<EmployeeDto?>;

public class GetEmployeeByUserIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetEmployeeByUserIdQuery, EmployeeDto?>
{
    public async Task<EmployeeDto?> Handle(GetEmployeeByUserIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.GetByUserIdAsync(request.UserId, cancellationToken);

        if (employee is null)
        {
            return null;
        }

        return new EmployeeDto(
            employee.Id,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.IsActive,
            employee.Roles.Select(role => role.Name).ToList());
    }
}