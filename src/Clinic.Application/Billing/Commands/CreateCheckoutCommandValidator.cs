using FluentValidation;

namespace Clinic.Application.Billing.Commands;

public sealed class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
{
    public CreateCheckoutCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.PlanCode).NotEmpty().MaximumLength(80);
        RuleFor(command => command.Provider).IsInEnum();
    }
}
