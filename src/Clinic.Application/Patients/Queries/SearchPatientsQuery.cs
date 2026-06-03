using Clinic.Application.Common.Models;
using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed record SearchPatientsQuery(
    Guid? TenantId,
    Guid? LocationId,
    string? Search,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<PatientDto>>;
