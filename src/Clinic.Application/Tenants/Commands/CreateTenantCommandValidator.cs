using FluentValidation;

namespace Clinic.Application.Tenants.Commands;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(120)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may contain lowercase letters, numbers, and hyphens.");
        RuleFor(command => command.Status).NotEmpty().MaximumLength(40);
        RuleFor(command => command.SettingsJson).NotEmpty().MaximumLength(8000);
    }
}
