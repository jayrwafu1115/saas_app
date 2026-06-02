using System.Security.Claims;
using Clinic.Application.Common.Security;

namespace Clinic.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string CreateAccessToken(JwtUser user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
