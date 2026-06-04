using MediatR;

namespace Clinic.Application.Reporting.Queries;

public sealed record GetReportingDashboardQuery(Guid? TenantId, DateOnly From, DateOnly To) : IRequest<ReportingDashboardDto>;
