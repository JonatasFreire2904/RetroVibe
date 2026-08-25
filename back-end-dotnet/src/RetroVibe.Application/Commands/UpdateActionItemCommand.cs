using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record UpdateActionItemCommand(
    string ActionItemId,
    ActionItemStatus? Status,
    string? Description,
    Optional<string?> AssigneeId,
    Optional<DateTime?> DueDate,
    string RequestedBy) : IRequest<Result<ActionItemDto>>;

public sealed class UpdateActionItemCommandHandler(
    IActionItemRepository actionItems, ISessionRepository sessions, IUserRepository users)
    : IRequestHandler<UpdateActionItemCommand, Result<ActionItemDto>>
{
    public async Task<Result<ActionItemDto>> Handle(UpdateActionItemCommand request, CancellationToken ct)
    {
        var item = await actionItems.FindByIdAsync(request.ActionItemId, ct);
        if (item is null) return Result<ActionItemDto>.Fail(DomainFailure.NotFound($"Action item {request.ActionItemId} not found"));

        var requester = await users.FindByIdAsync(request.RequestedBy, ct);
        if (requester is null) return Result<ActionItemDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var session = await sessions.FindByIdAsync(item.SessionId, ct);
        if (session is null) return Result<ActionItemDto>.Fail(DomainFailure.NotFound("Sessão do item de ação não encontrada"));

        var access = SquadAccessGuard.AssertSquadAccess(requester, session.SquadId);
        if (!access.IsOk) return Result<ActionItemDto>.FromFailure(access);

        if (request.Status is not null)
        {
            var result = item.ChangeStatus(request.Status.Value);
            if (!result.IsOk) return Result<ActionItemDto>.FromFailure(result);
        }

        if (request.Description is not null)
        {
            var result = item.UpdateDescription(request.Description);
            if (!result.IsOk) return Result<ActionItemDto>.FromFailure(result);
        }

        if (request.AssigneeId.IsSpecified || request.DueDate.IsSpecified)
        {
            var assigneeId = request.AssigneeId.IsSpecified ? request.AssigneeId.Value : item.AssigneeId;
            var dueDate = request.DueDate.IsSpecified ? request.DueDate.Value : item.DueDate;
            item.Reassign(assigneeId, dueDate);
        }

        await actionItems.SaveAsync(item, ct);

        var assignee = item.AssigneeId is null ? null : await users.FindByIdAsync(item.AssigneeId, ct);
        return Result<ActionItemDto>.Ok(ActionItemAssembler.ToActionItemDto(item, assignee));
    }
}
