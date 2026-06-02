using System.Security.Claims;
using Clinic.Application.Common.Interfaces;

namespace Clinic.Api.Extensions;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
