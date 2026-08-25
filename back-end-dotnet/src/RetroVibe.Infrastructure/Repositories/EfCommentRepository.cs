using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Infrastructure.Repositories;

public sealed class EfCommentRepository(RetroVibeDbContext db) : ICommentRepository
{
    public async Task SaveAsync(Comment comment, CancellationToken ct = default)
    {
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Comment>> ListByParentAsync(CommentParentKind parentKind, string parentId, CancellationToken ct = default)
    {
        return await db.Comments
            .Where(c => c.ParentKind == parentKind && c.ParentId == parentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
