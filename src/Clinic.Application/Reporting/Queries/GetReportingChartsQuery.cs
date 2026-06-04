using MediatR;

namespace Clinic.Application.Reporting.Queries;

public sealed record GetReportingChartsQuery(Guid? TenantId, DateOnly From, DateOnly To) : IRequest<ReportingChartsDto>;
