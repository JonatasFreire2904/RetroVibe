using System.Security.Claims;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Api.Auth;

public static class SquadScope
{
    public static string? EffectiveSquadId(ClaimsPrincipal user, RetroVibe.Domain.Entities.User? account, string? requestedSquadId)
    {
        if (user.GetAccessLevel() == AccessLevel.Admin) return requestedSquadId;
        if (account is null) return "__none__";
        if (requestedSquadId is not null)
            return account.CanAccessSquad(requestedSquadId) ? requestedSquadId : "__none__";
        return account.SquadId ?? account.ManagedSquadIds.FirstOrDefault() ?? "__none__";
    }
}
