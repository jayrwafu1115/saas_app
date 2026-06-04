using System.Net;
using Clinic.Api.Options;
using Microsoft.Extensions.Options;

namespace Clinic.Api.Extensions;

public sealed class IpRestrictionMiddleware(RequestDelegate next, IOptions<IpRestrictionOptions> options)
{
    private readonly IpRestrictionOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is null
            || MatchesAny(remoteIp, _options.Blocked)
            || (_options.Allowed.Length > 0 && !MatchesAny(remoteIp, _options.Allowed)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "IP address is not allowed." });
            return;
        }

        await next(context);
    }

    private static bool MatchesAny(IPAddress remoteIp, IEnumerable<string> rules) =>
        rules.Where(rule => !string.IsNullOrWhiteSpace(rule))
            .Any(rule => Matches(remoteIp, rule.Trim()));

    private static bool Matches(IPAddress remoteIp, string rule)
    {
        if (!rule.Contains('/'))
        {
            return IPAddress.TryParse(rule, out var address) && Normalize(remoteIp).Equals(Normalize(address));
        }

        var parts = rule.Split('/', 2);
        if (!IPAddress.TryParse(parts[0], out var network)
            || !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var addressBytes = Normalize(remoteIp).GetAddressBytes();
        var networkBytes = Normalize(network).GetAddressBytes();
        if (addressBytes.Length != networkBytes.Length || prefixLength < 0 || prefixLength > addressBytes.Length * 8)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;
        for (var index = 0; index < fullBytes; index++)
        {
            if (addressBytes[index] != networkBytes[index])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(byte.MaxValue << (8 - remainingBits));
        return (addressBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }

    private static IPAddress Normalize(IPAddress address) =>
        address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
}
