namespace APFMech.Domain.Entities;

public class EmployeeRole
{
    private EmployeeRole() { }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public static EmployeeRole Create(Guid employeeId, string name)
    {
        if (employeeId == Guid.Empty)
        {
            throw new ArgumentException("EmployeeId is required.", nameof(employeeId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name is required.", nameof(name));
        }

        return new EmployeeRole
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Name = name.Trim()
        };
    }
}