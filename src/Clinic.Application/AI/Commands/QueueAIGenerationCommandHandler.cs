using MediatR;

namespace Clinic.Application.AI.Commands;

public sealed class QueueAIGenerationCommandHandler(IAIService aiService)
    : IRequestHandler<QueueAIGenerationCommand, AIGenerationDto>
{
    public Task<AIGenerationDto> Handle(QueueAIGenerationCommand request, CancellationToken cancellationToken) =>
        aiService.QueueGenerationAsync(request.EncounterId, request.Type, request.Provider, request.Model, cancellationToken);
}
