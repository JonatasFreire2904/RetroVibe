using RetroVibe.Domain.Kernel;

namespace RetroVibe.Domain.Entities;

public sealed class RetroCard
{
    private const int MaxTextLength = 500;

    private readonly List<string> _voterIds;

    public string Id { get; private set; } = null!;
    public string ColumnId { get; private set; } = null!;
    public string AuthorId { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public IReadOnlyCollection<string> VoterIds => _voterIds;
    public int CommentsCount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public int VoteCount => _voterIds.Count;

    private RetroCard()
    {
        _voterIds = new List<string>();
    }

    private static Result<string> ValidateText(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return Result<string>.Fail(DomainFailure.Validation("O texto do card não pode estar vazio"));
        }
        if (trimmed.Length > MaxTextLength)
        {
            return Result<string>.Fail(DomainFailure.Validation($"O texto do card não pode ter mais de {MaxTextLength} caracteres"));
        }
        return Result<string>.Ok(trimmed);
    }

    public static Result<RetroCard> Create(string id, string columnId, string authorId, string text)
    {
        var validated = ValidateText(text);
        if (!validated.IsOk) return Result<RetroCard>.FromFailure(validated);

        return Result<RetroCard>.Ok(new RetroCard
        {
            Id = id,
            ColumnId = columnId,
            AuthorId = authorId,
            Text = validated.Value!,
            CommentsCount = 0,
            CreatedAt = DateTime.UtcNow,
        });
    }

    public static RetroCard Restore(
        string id, string columnId, string authorId, string text,
        IEnumerable<string> voterIds, int commentsCount, DateTime createdAt)
    {
        var card = new RetroCard
        {
            Id = id,
            ColumnId = columnId,
            AuthorId = authorId,
            Text = text,
            CommentsCount = commentsCount,
            CreatedAt = createdAt,
        };
        foreach (var voterId in voterIds)
        {
            if (!card._voterIds.Contains(voterId)) card._voterIds.Add(voterId);
        }
        return card;
    }

    public bool ToggleVote(string userId)
    {
        if (_voterIds.Contains(userId))
        {
            _voterIds.Remove(userId);
            return false;
        }
        _voterIds.Add(userId);
        return true;
    }

    public Result<Unit> UpdateText(string text)
    {
        var validated = ValidateText(text);
        if (!validated.IsOk) return Result<Unit>.FromFailure(validated);
        Text = validated.Value!;
        return Result<Unit>.Ok(Unit.Value);
    }

    public void RegisterComment() => CommentsCount++;
}
