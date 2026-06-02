using FluentValidation;

namespace Clinic.Application.Locations.Commands;

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Code).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Address).NotEmpty().MaximumLength(500);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(40);
    }
}
