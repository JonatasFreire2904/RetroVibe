using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record AddCardCommand(string SessionId, string ColumnId, string Text, string RequestedBy)
    : IRequest<Result<SessionCardDto>>;

public sealed class AddCardCommandHandler(ISessionRepository sessions, IUserRepository users)
    : IRequestHandler<AddCardCommand, Result<SessionCardDto>>
{
    public async Task<Result<SessionCardDto>> Handle(AddCardCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<SessionCardDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var author = await users.FindByIdAsync(request.RequestedBy, ct);
        if (author is null) return Result<SessionCardDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertCanContributeToSession(author, session);
        if (!access.IsOk) return Result<SessionCardDto>.FromFailure(access);

        var result = session.AddCard(Guid.NewGuid().ToString(), request.ColumnId, author.Id, request.Text);
        if (!result.IsOk) return Result<SessionCardDto>.FromFailure(result);

        await sessions.SaveAsync(session, ct);
        return Result<SessionCardDto>.Ok(SessionAssembler.ToSessionCardDto(result.Value!, session, author.Id));
    }
}
