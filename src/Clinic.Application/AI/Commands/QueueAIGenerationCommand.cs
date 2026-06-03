using Clinic.Domain.AI;
using MediatR;

namespace Clinic.Application.AI.Commands;

public sealed record QueueAIGenerationCommand(Guid EncounterId, AIGenerationType Type, string Provider, string? Model) : IRequest<AIGenerationDto>;
