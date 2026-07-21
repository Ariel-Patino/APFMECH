using APFMech.Application.Common.Interfaces;
using MediatR;

namespace APFMech.Application.Employees.Commands.DisableEmployee;

public class DisableEmployeeCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DisableEmployeeCommand, EmployeeDto?>
{
    public async Task<EmployeeDto?> Handle(DisableEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        employee.Deactivate();
        dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee.ToDto();
    }
}