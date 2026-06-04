using FluentValidation;

namespace Clinic.Application.Billing.Commands;

public sealed class StartTrialCommandValidator : AbstractValidator<StartTrialCommand>
{
    public StartTrialCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.PlanCode).NotEmpty().MaximumLength(80);
    }
}
