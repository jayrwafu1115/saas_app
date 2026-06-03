using FluentValidation;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddPrescriptionCommandValidator : AbstractValidator<AddPrescriptionCommand>
{
    public AddPrescriptionCommandValidator()
    {
        RuleFor(command => command.EncounterId).NotEmpty();
        RuleFor(command => command.MedicationName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Dosage).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Frequency).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Duration).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Instructions).MaximumLength(1000);
    }
}
