using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed class ListEncounterAIGenerationsQueryHandler(IAIGenerationRepository generations)
    : IRequestHandler<ListEncounterAIGenerationsQuery, IReadOnlyList<AIGenerationDto>>
{
    public async Task<IReadOnlyList<AIGenerationDto>> Handle(ListEncounterAIGenerationsQuery request, CancellationToken cancellationToken) =>
        (await generations.ListByEncounterAsync(request.EncounterId, cancellationToken)).Select(generation => generation.ToDto()).ToList();
}
