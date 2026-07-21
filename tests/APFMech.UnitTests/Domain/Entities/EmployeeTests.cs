using APFMech.Domain.Entities;

namespace APFMech.UnitTests.Domain.Entities;

public class EmployeeTests
{
    [Fact]
    public void Create_ShouldTrimNamesAndNormalizeRoles()
    {
        var userId = Guid.NewGuid();

        var employee = Employee.Create(userId, "  Alex  ", "  Turner ", [" Mechanic ", "Mechanic", "Inspector"]);

        Assert.Equal(userId, employee.UserId);
        Assert.Equal("Alex", employee.FirstName);
        Assert.Equal("Turner", employee.LastName);
        Assert.True(employee.IsActive);
        Assert.Collection(
            employee.Roles,
            role => Assert.Equal("Mechanic", role.Name),
            role => Assert.Equal("Inspector", role.Name));
        Assert.Contains(employee.Roles, role => role.Name == "Mechanic");
        Assert.Contains(employee.Roles, role => role.Name == "Inspector");
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() => Employee.Create(Guid.Empty, "Alex", "Turner"));

        Assert.StartsWith("UserId is required.", exception.Message);
    }

    [Fact]
    public void Deactivate_ShouldMarkEmployeeAsInactiveAndUpdateTimestamp()
    {
        var employee = Employee.Create(Guid.NewGuid(), "Alex", "Turner");

        employee.Deactivate();

        Assert.False(employee.IsActive);
        Assert.NotNull(employee.UpdatedAtUtc);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveExistingRoleAndIgnoreMissingRole()
    {
        var employee = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic", "Inspector"]);

        employee.RemoveRole("Inspector");
        employee.RemoveRole("MissingRole");

        Assert.Single(employee.Roles);
        Assert.Equal("Mechanic", employee.Roles.First().Name);
    }
}
