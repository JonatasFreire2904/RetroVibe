using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListActionItemCommentsQuery(string ActionItemId, string RequestedBy) : IRequest<Result<List<CommentDto>>>;

public sealed class ListActionItemCommentsQueryHandler(
    IActionItemRepository actionItems, ISessionRepository sessions, ICommentRepository comments, IUserRepository users)
    : IRequestHandler<ListActionItemCommentsQuery, Result<List<CommentDto>>>
{
    public async Task<Result<List<CommentDto>>> Handle(ListActionItemCommentsQuery request, CancellationToken ct)
    {
        var item = await actionItems.FindByIdAsync(request.ActionItemId, ct);
        if (item is null) return Result<List<CommentDto>>.Fail(DomainFailure.NotFound($"Action item {request.ActionItemId} not found"));

        var requester = await users.FindByIdAsync(request.RequestedBy, ct);
        if (requester is null) return Result<List<CommentDto>>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var session = await sessions.FindByIdAsync(item.SessionId, ct);
        if (session is null) return Result<List<CommentDto>>.Fail(DomainFailure.NotFound("Sessão do item de ação não encontrada"));

        var access = SquadAccessGuard.AssertSquadAccess(requester, session.SquadId);
        if (!access.IsOk) return Result<List<CommentDto>>.FromFailure(access);

        var list = await comments.ListByParentAsync(CommentParentKind.ActionItem, request.ActionItemId, ct);
        var dtos = new List<CommentDto>();
        foreach (var comment in list)
        {
            var author = await users.FindByIdAsync(comment.AuthorId, ct);
            dtos.Add(CommentAssembler.ToCommentDto(comment, author, mask: false));
        }
        return Result<List<CommentDto>>.Ok(dtos);
    }
}
