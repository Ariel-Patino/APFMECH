using MediatR;

namespace APFMech.Application.Employees.Commands.DeleteEmployeeGdpr;

public record DeleteEmployeeGdprCommand(Guid EmployeeId) : IRequest<bool>;