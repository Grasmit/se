using FluentValidation;
using DotNet.DtoVsDomain.Api.Contracts.Requests;

namespace DotNet.DtoVsDomain.Api.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.Items).NotNull().Must(x => x.Count > 0).WithMessage("At least one order item is required.");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}
