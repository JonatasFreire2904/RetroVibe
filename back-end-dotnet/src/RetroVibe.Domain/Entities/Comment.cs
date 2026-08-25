using RetroVibe.Domain.Kernel;

namespace RetroVibe.Domain.Entities;

public enum CommentParentKind
{
    Card,
    ActionItem,
}

public sealed class Comment
{
    public string Id { get; private set; } = null!;
    public CommentParentKind ParentKind { get; private set; }
    public string ParentId { get; private set; } = null!;
    public string AuthorId { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Comment() { }

    public static Result<Comment> Create(string id, CommentParentKind parentKind, string parentId, string authorId, string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return Result<Comment>.Fail(DomainFailure.Validation("O comentário não pode estar vazio"));
        }

        return Result<Comment>.Ok(new Comment
        {
            Id = id,
            ParentKind = parentKind,
            ParentId = parentId,
            AuthorId = authorId,
            Text = trimmed,
            CreatedAt = DateTime.UtcNow,
        });
    }

    public static Comment Restore(string id, CommentParentKind parentKind, string parentId, string authorId, string text, DateTime createdAt)
    {
        return new Comment
        {
            Id = id,
            ParentKind = parentKind,
            ParentId = parentId,
            AuthorId = authorId,
            Text = text,
            CreatedAt = createdAt,
        };
    }
}
