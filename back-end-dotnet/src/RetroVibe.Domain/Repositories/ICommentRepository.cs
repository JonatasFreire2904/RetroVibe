using RetroVibe.Domain.Entities;

namespace RetroVibe.Domain.Repositories;

public interface ICommentRepository
{
    Task SaveAsync(Comment comment, CancellationToken ct = default);
    Task<IReadOnlyList<Comment>> ListByParentAsync(CommentParentKind parentKind, string parentId, CancellationToken ct = default);
}
