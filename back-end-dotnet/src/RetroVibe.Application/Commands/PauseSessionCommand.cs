using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record PauseSessionCommand(string SessionId, string RequestedBy) : IRequest<Result<SessionBoardDto>>;

public sealed class PauseSessionCommandHandler(
    ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems, IUserRepository users)
    : IRequestHandler<PauseSessionCommand, Result<SessionBoardDto>>
{
    public async Task<Result<SessionBoardDto>> Handle(PauseSessionCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<SessionBoardDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var user = await users.FindByIdAsync(request.RequestedBy, ct);
        if (user is null) return Result<SessionBoardDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertSquadAccess(user, session.SquadId);
        if (!access.IsOk) return Result<SessionBoardDto>.FromFailure(access);

        var paused = session.Pause();
        if (!paused.IsOk) return Result<SessionBoardDto>.FromFailure(paused);

        await sessions.SaveAsync(session, ct);
        return await BoardAssembler.AssembleBoardAfterMutation(session, user.Id, catalog, actionItems, ct);
    }
}
