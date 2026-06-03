using FluentValidation;

namespace Clinic.Application.Patients.Commands;

public sealed class UpdatePatientContactCommandValidator : AbstractValidator<UpdatePatientContactCommand>
{
    public UpdatePatientContactCommandValidator()
    {
        RuleFor(command => command.PatientId).NotEmpty();
        RuleFor(command => command.ContactId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Relationship).NotEmpty().MaximumLength(80);
        RuleFor(command => command.Email).EmailAddress().MaximumLength(200).When(command => !string.IsNullOrWhiteSpace(command.Email));
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(40);
    }
}
