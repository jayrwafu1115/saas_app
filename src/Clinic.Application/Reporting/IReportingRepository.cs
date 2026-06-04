namespace Clinic.Application.Reporting;

public interface IReportingRepository
{
    Task<DashboardKpiDto> GetKpisAsync(Guid? tenantId, DateOnly from, DateOnly to, decimal visitRevenue, CancellationToken cancellationToken);
    Task<ReportingChartsDto> GetChartsAsync(Guid? tenantId, DateOnly from, DateOnly to, decimal visitRevenue, CancellationToken cancellationToken);
}
