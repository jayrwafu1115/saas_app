using MediatR;

namespace Clinic.Application.Clinical.Queries;

public sealed class GetEncounterByIdQueryHandler(IEncounterRepository encounters)
    : IRequestHandler<GetEncounterByIdQuery, EncounterDetailDto>
{
    public async Task<EncounterDetailDto> Handle(GetEncounterByIdQuery request, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(request.EncounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        return encounter.ToDetailDto();
    }
}
