using FluentValidation;

namespace APFMech.Application.WorkOrders.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    public CompleteWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work Order ID is required.");
    }
}