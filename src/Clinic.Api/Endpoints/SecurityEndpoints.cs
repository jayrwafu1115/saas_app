using Clinic.Application.Common.Security;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Api.Endpoints;

public static class SecurityEndpoints
{
    public static IEndpointRouteBuilder MapSecurityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/security")
            .WithTags("Security")
            .RequireAuthorization(AuthorizationPolicyNames.SuperAdminOnly);

        group.MapGet("/audit-logs", GetAuditLogsAsync)
            .WithName("GetSecurityAuditLogs")
            .Produces<SecurityAuditLogListResponse>();

        return app;
    }

    private static async Task<IResult> GetAuditLogsAsync(
        ApplicationDbContext dbContext,
        int page = 1,
        int pageSize = 50,
        Guid? tenantId = null,
        string? eventType = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = dbContext.SecurityAuditLogs.AsNoTracking();
        if (tenantId.HasValue)
        {
            query = query.Where(log => log.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(log => log.EventType == eventType);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(log => log.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new SecurityAuditLogResponse(
                log.Id,
                log.TenantId,
                log.UserId,
                log.EventType,
                log.Subject,
                log.IpAddress,
                log.UserAgent,
                log.DetailsJson,
                log.CreatedAtUtc))
            .ToListAsync();

        return Results.Ok(new SecurityAuditLogListResponse(items, page, pageSize, total));
    }

    private sealed record SecurityAuditLogListResponse(
        IReadOnlyCollection<SecurityAuditLogResponse> Items,
        int Page,
        int PageSize,
        int Total);

    private sealed record SecurityAuditLogResponse(
        Guid Id,
        Guid? TenantId,
        Guid? UserId,
        string EventType,
        string Subject,
        string IpAddress,
        string UserAgent,
        string DetailsJson,
        DateTimeOffset CreatedAtUtc);
}
