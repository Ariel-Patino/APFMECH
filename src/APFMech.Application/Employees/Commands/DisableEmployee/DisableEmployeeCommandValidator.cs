using FluentValidation;

namespace APFMech.Application.Employees.Commands.DisableEmployee;

public class DisableEmployeeCommandValidator : AbstractValidator<DisableEmployeeCommand>
{
    public DisableEmployeeCommandValidator()
    {
        RuleFor(command => command.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID is required.");
    }
}