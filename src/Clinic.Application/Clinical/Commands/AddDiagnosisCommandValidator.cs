using FluentValidation;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddDiagnosisCommandValidator : AbstractValidator<AddDiagnosisCommand>
{
    public AddDiagnosisCommandValidator()
    {
        RuleFor(command => command.EncounterId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(500);
        RuleFor(command => command.Type).NotEmpty().MaximumLength(80);
    }
}
