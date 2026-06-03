using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed record ListEncounterAIGenerationsQuery(Guid EncounterId) : IRequest<IReadOnlyList<AIGenerationDto>>;
