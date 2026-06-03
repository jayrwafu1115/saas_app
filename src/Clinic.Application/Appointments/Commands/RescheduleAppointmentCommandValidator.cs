using FluentValidation;

namespace Clinic.Application.Appointments.Commands;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.LocationId).NotEmpty();
        RuleFor(command => command.DoctorUserId).NotEmpty();
        RuleFor(command => command.StartsAtUtc).NotEmpty();
        RuleFor(command => command.EndsAtUtc).GreaterThan(command => command.StartsAtUtc);
    }
}
