using Clinic.Application.Locations;
using Clinic.Application.Patients;
using Clinic.Domain.Appointments;
using MediatR;

namespace Clinic.Application.Appointments.Commands;

public sealed class CreateAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IAvailabilityService availability,
    ILocationRepository locations,
    IPatientRepository patients)
    : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var locationExists = (await locations.ListAsync(request.TenantId, cancellationToken))
            .Any(location => location.Id == request.LocationId);
        if (!locationExists)
        {
            throw new InvalidOperationException("Location does not exist for this tenant.");
        }

        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null || patient.TenantId != request.TenantId)
        {
            throw new InvalidOperationException("Patient does not exist for this tenant.");
        }

        var result = await availability.CheckAvailabilityAsync(
            request.TenantId,
            request.LocationId,
            request.DoctorUserId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            null,
            cancellationToken);
        if (!result.IsAvailable)
        {
            throw new InvalidOperationException(result.Reason);
        }

        var appointment = new Appointment(
            request.TenantId,
            request.LocationId,
            request.PatientId,
            request.DoctorUserId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.Reason,
            request.Notes);

        await appointments.AddAsync(appointment, cancellationToken);
        return appointment.ToDto();
    }
}
