using FluentValidation;

namespace Clinic.Application.Patients.Commands;

public sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.LocationId).NotEmpty();
        RuleFor(command => command.MedicalRecordNumber).NotEmpty().MaximumLength(60);
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(120);
        RuleFor(command => command.MiddleName).MaximumLength(120);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(120);
        RuleFor(command => command.BirthDate).NotEmpty();
        RuleFor(command => command.Gender).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(40);
        RuleFor(command => command.Address).NotEmpty().MaximumLength(500);
    }
}
