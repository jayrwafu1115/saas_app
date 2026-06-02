using System.Security.Cryptography;
using System.Text;

namespace Clinic.Infrastructure.Identity;

public static class RefreshTokenGenerator
{
    public static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
