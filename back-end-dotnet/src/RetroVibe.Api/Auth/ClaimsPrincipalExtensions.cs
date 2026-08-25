using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RetroVibe.Application.Ports;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new InvalidOperationException("Missing sub claim");

    public static AccessLevel GetAccessLevel(this ClaimsPrincipal user) =>
        Enum.Parse<AccessLevel>(user.FindFirstValue(AuthClaimTypes.AccessLevel) ?? throw new InvalidOperationException("Missing access_level claim"));

    public static string? GetSquadId(this ClaimsPrincipal user) => user.FindFirstValue(AuthClaimTypes.SquadId);

    public static string? GetAllowedSessionId(this ClaimsPrincipal user) => user.FindFirstValue(AuthClaimTypes.AllowedSessionId);
}
