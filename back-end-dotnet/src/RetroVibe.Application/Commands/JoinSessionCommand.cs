using FluentValidation;
using MediatR;
using RetroVibe.Application.Ports;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record JoinSessionParticipantDto(string Id, string Name, string SessionId);

public sealed record JoinSessionResultDto(string Token, JoinSessionParticipantDto Participant);

public sealed record JoinSessionCommand(string SessionId, string DisplayName) : IRequest<Result<JoinSessionResultDto>>;

public sealed class JoinSessionCommandValidator : AbstractValidator<JoinSessionCommand>
{
    public JoinSessionCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(60);
    }
}

public sealed class JoinSessionCommandHandler(ISessionRepository sessions, IUserRepository users, IJwtTokenService tokens)
    : IRequestHandler<JoinSessionCommand, Result<JoinSessionResultDto>>
{
    public async Task<Result<JoinSessionResultDto>> Handle(JoinSessionCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result<JoinSessionResultDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));
        }
        if (session.Status != SessionStatus.Active)
        {
            return Result<JoinSessionResultDto>.Fail(DomainFailure.Conflict("Esta sessão não está ativa"));
        }

        var avatarColor = AvatarColors.PickDeterministic(request.DisplayName + request.SessionId);
        var guest = User.Restore(
            Guid.NewGuid().ToString(), request.DisplayName, "Participante", session.SquadId, avatarColor,
            username: null, passwordHash: null, AccessLevel.Participant, allowedSessionId: session.Id);
        await users.SaveAsync(guest, ct);

        session.Join();
        await sessions.SaveAsync(session, ct);

        var token = tokens.IssueToken(guest);
        return Result<JoinSessionResultDto>.Ok(new JoinSessionResultDto(token, new JoinSessionParticipantDto(guest.Id, guest.Name, session.Id)));
    }
}
