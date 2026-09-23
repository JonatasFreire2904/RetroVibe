using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record NavigateStageCommand(string SessionId, SessionPhase Phase, int ActiveColumnIndex, string RequestedBy)
    : IRequest<Result<SessionBoardDto>>;

public sealed class NavigateStageCommandHandler(
    ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems, IUserRepository users)
    : IRequestHandler<NavigateStageCommand, Result<SessionBoardDto>>
{
    public async Task<Result<SessionBoardDto>> Handle(NavigateStageCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<SessionBoardDto>.Fail(DomainFailure.NotFound("Sessão não encontrada"));
        var user = await users.FindByIdAsync(request.RequestedBy, ct);
        if (user is null) return Result<SessionBoardDto>.Fail(DomainFailure.Unauthorized("Usuário não encontrado"));
        var access = SquadAccessGuard.AssertSquadAccess(user, session.SquadId);
        if (!access.IsOk) return Result<SessionBoardDto>.FromFailure(access);

        var changed = session.NavigateTo(request.Phase, request.ActiveColumnIndex);
        if (!changed.IsOk) return Result<SessionBoardDto>.FromFailure(changed);
        await sessions.SaveAsync(session, ct);
        return await BoardAssembler.AssembleBoardAfterMutation(session, user.Id, catalog, actionItems, ct);
    }
}
