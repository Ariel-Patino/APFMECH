using MediatR;

namespace APFMech.Application.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles) : IRequest<EmployeeDto>;