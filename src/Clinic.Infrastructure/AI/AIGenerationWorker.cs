using Clinic.Application.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.AI;

public sealed class AIGenerationWorker(
    IAIGenerationQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AIGenerationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid generationId;
            try
            {
                generationId = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();
                await aiService.ProcessGenerationAsync(generationId, stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "AI generation {GenerationId} failed in worker.", generationId);
            }
        }
    }
}
