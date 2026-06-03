using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed record GetAIUsageQuery(Guid TenantId) : IRequest<AIUsageSummaryDto>;
