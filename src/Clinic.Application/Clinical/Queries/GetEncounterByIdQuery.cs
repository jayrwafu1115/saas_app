using MediatR;

namespace Clinic.Application.Clinical.Queries;

public sealed record GetEncounterByIdQuery(Guid EncounterId) : IRequest<EncounterDetailDto>;
