using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Support;

public static class ActionItemAssembler
{
    public static ActionItemDto ToActionItemDto(ActionItem item, User? assignee) => new(
        item.Id, item.SessionId, item.Description, item.Status,
        assignee is null ? null : new ActionItemAssigneeDto(assignee.Id, assignee.Name, assignee.AvatarColor),
        item.DueDate, item.CommentsCount, item.CreatedAt);
}
