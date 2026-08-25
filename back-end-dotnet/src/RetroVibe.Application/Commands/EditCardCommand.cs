using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record EditCardCommand(string SessionId, string CardId, string Text, string RequestedBy)
    : IRequest<Result<SessionBoardDto>>;

public sealed class EditCardCommandHandler(
    ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems, IUserRepository users)
    : IRequestHandler<EditCardCommand, Result<SessionBoardDto>>
{
    public async Task<Result<SessionBoardDto>> Handle(EditCardCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<SessionBoardDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var user = await users.FindByIdAsync(request.RequestedBy, ct);
        if (user is null) return Result<SessionBoardDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertCanContributeToSession(user, session);
        if (!access.IsOk) return Result<SessionBoardDto>.FromFailure(access);

        var edited = session.EditCard(request.CardId, user.Id, request.Text);
        if (!edited.IsOk) return Result<SessionBoardDto>.FromFailure(edited);

        await sessions.SaveAsync(session, ct);
        return await BoardAssembler.AssembleBoardAfterMutation(session, user.Id, catalog, actionItems, ct);
    }
}
