using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record AddActionItemCommentCommand(string ActionItemId, string Text, string RequestedBy)
    : IRequest<Result<CommentDto>>;

public sealed class AddActionItemCommentCommandHandler(
    IActionItemRepository actionItems, ISessionRepository sessions, ICommentRepository comments, IUserRepository users)
    : IRequestHandler<AddActionItemCommentCommand, Result<CommentDto>>
{
    public async Task<Result<CommentDto>> Handle(AddActionItemCommentCommand request, CancellationToken ct)
    {
        var item = await actionItems.FindByIdAsync(request.ActionItemId, ct);
        if (item is null) return Result<CommentDto>.Fail(DomainFailure.NotFound($"Action item {request.ActionItemId} not found"));

        var author = await users.FindByIdAsync(request.RequestedBy, ct);
        if (author is null) return Result<CommentDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var session = await sessions.FindByIdAsync(item.SessionId, ct);
        if (session is null) return Result<CommentDto>.Fail(DomainFailure.NotFound("Sessão do item de ação não encontrada"));

        var access = SquadAccessGuard.AssertSquadAccess(author, session.SquadId);
        if (!access.IsOk) return Result<CommentDto>.FromFailure(access);

        var comment = Comment.Create(Guid.NewGuid().ToString(), CommentParentKind.ActionItem, item.Id, author.Id, request.Text);
        if (!comment.IsOk) return Result<CommentDto>.FromFailure(comment);

        await comments.SaveAsync(comment.Value!, ct);
        item.RegisterComment();
        await actionItems.SaveAsync(item, ct);

        return Result<CommentDto>.Ok(CommentAssembler.ToCommentDto(comment.Value!, author, mask: false));
    }
}
