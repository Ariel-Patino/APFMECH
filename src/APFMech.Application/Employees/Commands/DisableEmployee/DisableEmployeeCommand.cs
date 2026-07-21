using MediatR;

namespace APFMech.Application.Employees.Commands.DisableEmployee;

public record DisableEmployeeCommand(Guid EmployeeId) : IRequest<EmployeeDto?>;