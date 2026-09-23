using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Dtos;

public sealed record ActionItemAssigneeDto(string Id, string Name, string AvatarColor);

public sealed record ActionItemDto(
    string Id, string SessionId, string Description, ActionItemStatus Status,
    ActionItemAssigneeDto? Assignee, DateTime? DueDate, int CommentsCount, DateTime CreatedAt);

public sealed record ActionItemSessionSummaryDto(
    string SessionId, string TemplateLabel, string TemplateIcon, string ThemeIcon, DateTime Date, int TotalItems, int CompletedItems);
