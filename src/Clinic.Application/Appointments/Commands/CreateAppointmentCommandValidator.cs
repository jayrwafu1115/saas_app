using FluentValidation;

namespace Clinic.Application.Appointments.Commands;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty();
        RuleFor(command => command.LocationId).NotEmpty();
        RuleFor(command => command.PatientId).NotEmpty();
        RuleFor(command => command.DoctorUserId).NotEmpty();
        RuleFor(command => command.StartsAtUtc).NotEmpty();
        RuleFor(command => command.EndsAtUtc).GreaterThan(command => command.StartsAtUtc);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(250);
        RuleFor(command => command.Notes).MaximumLength(1000);
    }
}
