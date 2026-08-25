using MediatR;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record ToggleVoteResultDto(bool Voted, int Votes);

public sealed record ToggleVoteCommand(string SessionId, string CardId, string RequestedBy)
    : IRequest<Result<ToggleVoteResultDto>>;

public sealed class ToggleVoteCommandHandler(ISessionRepository sessions, IUserRepository users)
    : IRequestHandler<ToggleVoteCommand, Result<ToggleVoteResultDto>>
{
    public async Task<Result<ToggleVoteResultDto>> Handle(ToggleVoteCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<ToggleVoteResultDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var user = await users.FindByIdAsync(request.RequestedBy, ct);
        if (user is null) return Result<ToggleVoteResultDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertCanContributeToSession(user, session);
        if (!access.IsOk) return Result<ToggleVoteResultDto>.FromFailure(access);

        var toggled = session.ToggleVote(request.CardId, user.Id);
        if (!toggled.IsOk) return Result<ToggleVoteResultDto>.FromFailure(toggled);

        await sessions.SaveAsync(session, ct);

        var card = session.Columns.SelectMany(c => c.Cards).First(c => c.Id == request.CardId);
        return Result<ToggleVoteResultDto>.Ok(new ToggleVoteResultDto(toggled.Value, card.VoteCount));
    }
}
