using APFMech.Application.Employees.Commands.DeleteEmployeeGdpr;
using APFMech.Application.Employees.Commands.DisableEmployee;

namespace APFMech.UnitTests.Application.Employees.Commands;

public class EmployeeCommandValidatorTests
{
    [Fact]
    public void DisableEmployeeValidator_ShouldRejectEmptyEmployeeId()
    {
        var validator = new DisableEmployeeCommandValidator();

        var result = validator.Validate(new DisableEmployeeCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DisableEmployeeCommand.EmployeeId));
    }

    [Fact]
    public void DisableEmployeeValidator_ShouldAcceptValidEmployeeId()
    {
        var validator = new DisableEmployeeCommandValidator();

        var result = validator.Validate(new DisableEmployeeCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeleteEmployeeValidator_ShouldRejectEmptyEmployeeId()
    {
        var validator = new DeleteEmployeeGdprCommandValidator();

        var result = validator.Validate(new DeleteEmployeeGdprCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteEmployeeGdprCommand.EmployeeId));
    }

    [Fact]
    public void DeleteEmployeeValidator_ShouldAcceptValidEmployeeId()
    {
        var validator = new DeleteEmployeeGdprCommandValidator();

        var result = validator.Validate(new DeleteEmployeeGdprCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }
}
