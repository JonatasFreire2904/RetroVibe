using System.Text.Json.Serialization;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Domain.Entities;

public enum ActionItemStatus
{
    [JsonStringEnumMemberName("PLANNED")] Planned,
    [JsonStringEnumMemberName("BACKLOG")] Backlog,
    [JsonStringEnumMemberName("IN_PROGRESS")] InProgress,
    [JsonStringEnumMemberName("DONE")] Done,
    [JsonStringEnumMemberName("DISCARDED")] Discarded,
}

public sealed class ActionItem
{
    private const int MaxDescriptionLength = 280;

    public string Id { get; private set; } = null!;
    public string SessionId { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ActionItemStatus Status { get; private set; }
    public string? AssigneeId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public int CommentsCount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ActionItem() { }

    public static Result<ActionItem> Create(string id, string sessionId, string description, string? assigneeId, DateTime? dueDate)
    {
        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return Result<ActionItem>.Fail(DomainFailure.Validation("A descrição não pode estar vazia"));
        }
        if (trimmed.Length > MaxDescriptionLength)
        {
            return Result<ActionItem>.Fail(DomainFailure.Validation($"A descrição não pode ter mais de {MaxDescriptionLength} caracteres"));
        }

        return Result<ActionItem>.Ok(new ActionItem
        {
            Id = id,
            SessionId = sessionId,
            Description = trimmed,
            Status = ActionItemStatus.Planned,
            AssigneeId = assigneeId,
            DueDate = dueDate,
            CommentsCount = 0,
            CreatedAt = DateTime.UtcNow,
        });
    }

    public static ActionItem Restore(
        string id, string sessionId, string description, ActionItemStatus status,
        string? assigneeId, DateTime? dueDate, int commentsCount, DateTime createdAt)
    {
        return new ActionItem
        {
            Id = id,
            SessionId = sessionId,
            Description = description,
            Status = status,
            AssigneeId = assigneeId,
            DueDate = dueDate,
            CommentsCount = commentsCount,
            CreatedAt = createdAt,
        };
    }

    public Result<Unit> ChangeStatus(ActionItemStatus status)
    {
        Status = status;
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> UpdateDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return Result<Unit>.Fail(DomainFailure.Validation("A descrição não pode estar vazia"));
        }
        Description = trimmed;
        return Result<Unit>.Ok(Unit.Value);
    }

    public void Reassign(string? assigneeId, DateTime? dueDate)
    {
        AssigneeId = assigneeId;
        DueDate = dueDate;
    }

    public void RegisterComment() => CommentsCount++;
}
