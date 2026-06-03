using FluentValidation;

namespace Clinic.Application.Patients.Commands;

public sealed class UploadPatientDocumentCommandValidator : AbstractValidator<UploadPatientDocumentCommand>
{
    private const long MaxSizeBytes = 25 * 1024 * 1024;

    public UploadPatientDocumentCommandValidator()
    {
        RuleFor(command => command.PatientId).NotEmpty();
        RuleFor(command => command.FileName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.ContentType).NotEmpty().MaximumLength(120);
        RuleFor(command => command.SizeBytes).GreaterThan(0).LessThanOrEqualTo(MaxSizeBytes);
    }
}
