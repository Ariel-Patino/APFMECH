using APFMech.Domain.Entities;

namespace APFMech.Application.Employees;

internal static class EmployeeDtoMapper
{
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto(
            employee.Id,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.IsActive,
            employee.Roles.Select(role => role.Name).ToList());
    }
}