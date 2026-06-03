using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed class GetAIUsageQueryHandler(IAIGenerationRepository generations)
    : IRequestHandler<GetAIUsageQuery, AIUsageSummaryDto>
{
    public Task<AIUsageSummaryDto> Handle(GetAIUsageQuery request, CancellationToken cancellationToken) =>
        generations.GetUsageAsync(request.TenantId, cancellationToken);
}
