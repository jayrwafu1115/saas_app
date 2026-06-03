using MediatR;

namespace Clinic.Application.AI.Queries;

public sealed record GetAIGenerationQuery(Guid Id) : IRequest<AIGenerationDto>;
