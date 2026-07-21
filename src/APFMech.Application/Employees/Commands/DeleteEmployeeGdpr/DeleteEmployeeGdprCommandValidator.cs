using FluentValidation;

namespace APFMech.Application.Employees.Commands.DeleteEmployeeGdpr;

public class DeleteEmployeeGdprCommandValidator : AbstractValidator<DeleteEmployeeGdprCommand>
{
    public DeleteEmployeeGdprCommandValidator()
    {
        RuleFor(command => command.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID is required.");
    }
}