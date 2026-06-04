using MediatR;

namespace Clinic.Application.Reporting.Queries;

public sealed class GetReportingDashboardQueryHandler(IReportingService reporting)
    : IRequestHandler<GetReportingDashboardQuery, ReportingDashboardDto>
{
    public Task<ReportingDashboardDto> Handle(GetReportingDashboardQuery request, CancellationToken cancellationToken) =>
        reporting.GetDashboardAsync(request.TenantId, request.From, request.To, cancellationToken);
}
