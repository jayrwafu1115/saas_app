using Clinic.Application.Common.Interfaces;
using Clinic.Application.Common.Models;
using MediatR;

namespace Clinic.Application.Patients.Queries;

public sealed class SearchPatientsQueryHandler(IPatientRepository patients, ICurrentTenant currentTenant)
    : IRequestHandler<SearchPatientsQuery, PagedResult<PatientDto>>
{
    public async Task<PagedResult<PatientDto>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is <= 0 or > 100 ? 20 : request.PageSize;
        var tenantId = request.TenantId ?? currentTenant.TenantId;
        var results = await patients.SearchAsync(
            tenantId,
            request.LocationId,
            request.Search,
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResult<PatientDto>(
            results.Items.Select(patient => patient.ToDto()).ToList(),
            results.PageNumber,
            results.PageSize,
            results.TotalCount);
    }
}
