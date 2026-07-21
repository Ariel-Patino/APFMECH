using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using APFMech.Domain.Entities;

namespace APFMech.Application.WorkOrders;

internal static class WorkOrderDtoMapper
{
    public static async Task<WorkOrderDto> ToDtoAsync(
        this WorkOrder workOrder,
        IEmployeeRepository employeeRepository,
        CancellationToken cancellationToken)
    {
        var assignedMechanicFullName = await ResolveAssignedMechanicFullNameAsync(
            workOrder.AssignedMechanicId,
            employeeRepository,
            cancellationToken);

        return new WorkOrderDto(
            workOrder.Id,
            workOrder.TrackingNumber,
            workOrder.Description,
            workOrder.Status.ToString(),
            workOrder.AssignedMechanicId,
            assignedMechanicFullName,
            workOrder.CreatedAtUtc);
    }

    private static async Task<string?> ResolveAssignedMechanicFullNameAsync(
        Guid? assignedMechanicId,
        IEmployeeRepository employeeRepository,
        CancellationToken cancellationToken)
    {
        if (!assignedMechanicId.HasValue)
        {
            return null;
        }

        var employee = await employeeRepository.GetByIdAsync(assignedMechanicId.Value, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        return $"{employee.FirstName} {employee.LastName}".Trim();
    }
}