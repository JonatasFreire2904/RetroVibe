using MediatR;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record AddCardCommentResultDto(int CommentsCount);

public sealed record AddCardCommentCommand(string SessionId, string CardId, string Text, string RequestedBy)
    : IRequest<Result<AddCardCommentResultDto>>;

public sealed class AddCardCommentCommandHandler(ISessionRepository sessions, ICommentRepository comments, IUserRepository users)
    : IRequestHandler<AddCardCommentCommand, Result<AddCardCommentResultDto>>
{
    public async Task<Result<AddCardCommentResultDto>> Handle(AddCardCommentCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<AddCardCommentResultDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var author = await users.FindByIdAsync(request.RequestedBy, ct);
        if (author is null) return Result<AddCardCommentResultDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertCanContributeToSession(author, session);
        if (!access.IsOk) return Result<AddCardCommentResultDto>.FromFailure(access);

        var registered = session.RegisterCardComment(request.CardId);
        if (!registered.IsOk) return Result<AddCardCommentResultDto>.FromFailure(registered);

        var comment = Comment.Create(Guid.NewGuid().ToString(), CommentParentKind.Card, request.CardId, author.Id, request.Text);
        if (!comment.IsOk) return Result<AddCardCommentResultDto>.FromFailure(comment);

        await comments.SaveAsync(comment.Value!, ct);
        await sessions.SaveAsync(session, ct);

        var card = session.Columns.SelectMany(c => c.Cards).First(c => c.Id == request.CardId);
        return Result<AddCardCommentResultDto>.Ok(new AddCardCommentResultDto(card.CommentsCount));
    }
}
