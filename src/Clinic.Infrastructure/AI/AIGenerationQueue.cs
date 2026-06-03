using System.Threading.Channels;
using Clinic.Application.AI;

namespace Clinic.Infrastructure.AI;

public sealed class AIGenerationQueue : IAIGenerationQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid generationId, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(generationId, cancellationToken);

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);
}
