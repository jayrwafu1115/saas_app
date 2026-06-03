using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed class GetPatientByIdQueryHandler(IPatientRepository patients)
    : IRequestHandler<GetPatientByIdQuery, PatientDetailDto>
{
    public async Task<PatientDetailDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        return patient.ToDetailDto();
    }
}
