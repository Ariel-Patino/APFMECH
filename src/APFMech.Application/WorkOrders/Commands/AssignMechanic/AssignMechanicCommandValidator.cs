using FluentValidation;

namespace APFMech.Application.WorkOrders.Commands.AssignMechanic;

public class AssignMechanicCommandValidator : AbstractValidator<AssignMechanicCommand>  
{
public AssignMechanicCommandValidator()
{
RuleFor(x => x.WorkOrderId)
.NotEmpty().WithMessage("Work Order ID is required.");

    RuleFor(x => x.MechanicId)
        .NotEmpty().WithMessage("Mechanic ID is required.");
}


}