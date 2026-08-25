using System.Security.Claims;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Api.Auth;

public static class SquadScope
{
    public static string? EffectiveSquadId(ClaimsPrincipal user, string? requestedSquadId)
    {
        if (user.GetAccessLevel() == AccessLevel.Admin) return requestedSquadId;
        return user.GetSquadId() ?? "__none__";
    }
}
