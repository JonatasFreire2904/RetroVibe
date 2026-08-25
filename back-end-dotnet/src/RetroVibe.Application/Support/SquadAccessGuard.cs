using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Application.Support;

public static class SquadAccessGuard
{
    public static Result<Unit> AssertSquadAccess(User user, string squadId)
    {
        return user.CanAccessSquad(squadId)
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.Fail(DomainFailure.Forbidden("Você não tem acesso a este squad"));
    }

    public static Result<Unit> AssertCanContributeToSession(User user, RetroSession session)
    {
        var allowed = user.CanAccessSquad(session.SquadId) || user.CanAccessSession(session.Id);
        return allowed
            ? Result<Unit>.Ok(Unit.Value)
            : Result<Unit>.Fail(DomainFailure.Forbidden("Você não tem acesso a esta sessão"));
    }
}
