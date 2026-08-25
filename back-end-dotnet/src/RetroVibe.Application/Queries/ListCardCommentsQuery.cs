using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListCardCommentsQuery(string SessionId, string CardId) : IRequest<List<CommentDto>?>;

public sealed class ListCardCommentsQueryHandler(ISessionRepository sessions, ICommentRepository comments, IUserRepository users)
    : IRequestHandler<ListCardCommentsQuery, List<CommentDto>?>
{
    public async Task<List<CommentDto>?> Handle(ListCardCommentsQuery request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return null;

        var mask = session.PrivacyMode == PrivacyMode.Anonymous;
        var list = await comments.ListByParentAsync(CommentParentKind.Card, request.CardId, ct);

        var dtos = new List<CommentDto>();
        foreach (var comment in list)
        {
            var author = mask ? null : await users.FindByIdAsync(comment.AuthorId, ct);
            dtos.Add(CommentAssembler.ToCommentDto(comment, author, mask));
        }
        return dtos;
    }
}
