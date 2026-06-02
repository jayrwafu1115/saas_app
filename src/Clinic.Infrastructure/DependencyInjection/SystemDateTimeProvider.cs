using Clinic.Application.Common.Interfaces;

namespace Clinic.Infrastructure.DependencyInjection;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
