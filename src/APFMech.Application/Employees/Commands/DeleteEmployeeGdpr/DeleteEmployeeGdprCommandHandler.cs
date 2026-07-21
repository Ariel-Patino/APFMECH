using APFMech.Application.Common.Interfaces;
using MediatR;

namespace APFMech.Application.Employees.Commands.DeleteEmployeeGdpr;

public class DeleteEmployeeGdprCommandHandler(
    IApplicationDbContext dbContext,
    IIdentityService identityService) : IRequestHandler<DeleteEmployeeGdprCommand, bool>
{
    public async Task<bool> Handle(DeleteEmployeeGdprCommand request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            return false;
        }

        var userDeleted = await identityService.DeleteUserAsync(employee.UserId);
        if (!userDeleted)
        {
            return false;
        }

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}