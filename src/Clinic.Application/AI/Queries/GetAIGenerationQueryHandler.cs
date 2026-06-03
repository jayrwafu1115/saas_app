using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed class GetAIGenerationQueryHandler(IAIGenerationRepository generations)
    : IRequestHandler<GetAIGenerationQuery, AIGenerationDto>
{
    public async Task<AIGenerationDto> Handle(GetAIGenerationQuery request, CancellationToken cancellationToken)
    {
        var generation = await generations.GetByIdAsync(request.Id, cancellationToken);
        if (generation is null)
        {
            throw new KeyNotFoundException("AI generation was not found.");
        }

        return generation.ToDto();
    }
}
