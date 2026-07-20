using APFMech.Application.Common.Interfaces;
using MediatR;

namespace APFMech.Application.Employees.Queries.GetAllEmployees;

public record GetAllEmployeesQuery : IRequest<IReadOnlyList<EmployeeDto>>;

public class GetAllEmployeesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    public async Task<IReadOnlyList<EmployeeDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await dbContext.Employees.GetAllAsync(cancellationToken);

        return employees
            .Select(employee => new EmployeeDto(
                employee.Id,
                employee.UserId,
                employee.FirstName,
                employee.LastName,
                employee.IsActive,
                employee.Roles.Select(role => role.Name).ToList()))
            .ToList();
    }
}