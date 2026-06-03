using FluentValidation;

namespace Clinic.Application.Clinical.Commands;

public sealed class CreateEncounterCommandValidator : AbstractValidator<CreateEncounterCommand>
{
    public CreateEncounterCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.LocationId).NotEmpty();
        RuleFor(command => command.PatientId).NotEmpty();
        RuleFor(command => command.ClinicianUserId).NotEmpty();
        RuleFor(command => command.EncounterDateUtc).NotEmpty();
        RuleFor(command => command.ChiefComplaint).NotEmpty().MaximumLength(500);
        RuleFor(command => command.Subjective).MaximumLength(4000);
        RuleFor(command => command.Objective).MaximumLength(4000);
        RuleFor(command => command.Assessment).MaximumLength(4000);
        RuleFor(command => command.Plan).MaximumLength(4000);
        RuleFor(command => command.Notes).MaximumLength(4000);
    }
}
