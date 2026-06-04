using Microsoft.Extensions.Options;

namespace Clinic.Application.Reporting;

public sealed class ReportingService(
    IReportingRepository reports,
    IReportingCache cache,
    IOptions<ReportingOptions> options)
    : IReportingService
{
    public async Task<ReportingDashboardDto> GetDashboardAsync(Guid? tenantId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var key = $"reports:dashboard:{tenantId}:{from:yyyyMMdd}:{to:yyyyMMdd}";
        var cached = await cache.GetAsync<ReportingDashboardDto>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var visitRevenue = options.Value.DefaultVisitRevenue;
        var dashboard = new ReportingDashboardDto(
            await reports.GetKpisAsync(tenantId, from, to, visitRevenue, cancellationToken),
            await reports.GetChartsAsync(tenantId, from, to, visitRevenue, cancellationToken));
        await cache.SetAsync(key, dashboard, TimeSpan.FromMinutes(options.Value.CacheMinutes), cancellationToken);
        return dashboard;
    }

    public async Task<ReportingChartsDto> GetChartsAsync(Guid? tenantId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var key = $"reports:charts:{tenantId}:{from:yyyyMMdd}:{to:yyyyMMdd}";
        var cached = await cache.GetAsync<ReportingChartsDto>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var charts = await reports.GetChartsAsync(tenantId, from, to, options.Value.DefaultVisitRevenue, cancellationToken);
        await cache.SetAsync(key, charts, TimeSpan.FromMinutes(options.Value.CacheMinutes), cancellationToken);
        return charts;
    }
}
