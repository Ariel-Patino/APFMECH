using APFMech.Domain.Common;

namespace APFMech.Domain.Entities;

public class Employee : BaseEntity
{
    private readonly List<EmployeeRole> _roles = [];

    private Employee() { }

    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<EmployeeRole> Roles => _roles.AsReadOnly();

    public static Employee Create(Guid userId, string firstName, string lastName, IEnumerable<string>? roles = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = true
        };

        foreach (var role in roles ?? [])
        {
            employee.AssignRole(role);
        }

        return employee;
    }

    public void AssignRole(string roleName)
    {
        var normalizedRole = NormalizeRoleName(roleName);

        if (_roles.Any(role => role.Name == normalizedRole))
        {
            return;
        }

        _roles.Add(EmployeeRole.Create(Id, normalizedRole));
        MarkAsUpdated();
    }

    public void RemoveRole(string roleName)
    {
        var normalizedRole = NormalizeRoleName(roleName);
        var role = _roles.FirstOrDefault(x => x.Name == normalizedRole);

        if (role is null)
        {
            return;
        }

        _roles.Remove(role);
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    private static string NormalizeRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name is required.", nameof(roleName));
        }

        return roleName.Trim();
    }
}