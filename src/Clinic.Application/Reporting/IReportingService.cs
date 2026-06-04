namespace Clinic.Application.Reporting;

public interface IReportingService
{
    Task<ReportingDashboardDto> GetDashboardAsync(Guid? tenantId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
    Task<ReportingChartsDto> GetChartsAsync(Guid? tenantId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
}
