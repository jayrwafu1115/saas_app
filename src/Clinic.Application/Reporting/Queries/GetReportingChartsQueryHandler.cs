using MediatR;

namespace Clinic.Application.Reporting.Queries;

public sealed class GetReportingChartsQueryHandler(IReportingService reporting)
    : IRequestHandler<GetReportingChartsQuery, ReportingChartsDto>
{
    public Task<ReportingChartsDto> Handle(GetReportingChartsQuery request, CancellationToken cancellationToken) =>
        reporting.GetChartsAsync(request.TenantId, request.From, request.To, cancellationToken);
}
