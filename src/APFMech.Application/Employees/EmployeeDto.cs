namespace APFMech.Application.Employees;

public record EmployeeDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    bool IsActive,
    IReadOnlyList<string> Roles);