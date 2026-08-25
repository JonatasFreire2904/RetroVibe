using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record CloseSessionCommand(string SessionId, double? FeedbackScore, string RequestedBy)
    : IRequest<Result<SessionBoardDto>>;

public sealed class CloseSessionCommandHandler(
    ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems, IUserRepository users)
    : IRequestHandler<CloseSessionCommand, Result<SessionBoardDto>>
{
    public async Task<Result<SessionBoardDto>> Handle(CloseSessionCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<SessionBoardDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var user = await users.FindByIdAsync(request.RequestedBy, ct);
        if (user is null) return Result<SessionBoardDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertSquadAccess(user, session.SquadId);
        if (!access.IsOk) return Result<SessionBoardDto>.FromFailure(access);

        var closed = session.Close(new CloseSessionInput(request.FeedbackScore));
        if (!closed.IsOk) return Result<SessionBoardDto>.FromFailure(closed);

        await sessions.SaveAsync(session, ct);
        return await BoardAssembler.AssembleBoardAfterMutation(session, user.Id, catalog, actionItems, ct);
    }
}
