using APFMech.Application.Common.Interfaces;
using APFMech.Domain.Entities;
using MediatR;

namespace APFMech.Application.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = Employee.Create(request.UserId, request.FirstName, request.LastName, request.Roles);

        await dbContext.Employees.AddAsync(employee, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new EmployeeDto(
            employee.Id,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.IsActive,
            employee.Roles.Select(role => role.Name).ToList());
    }
}