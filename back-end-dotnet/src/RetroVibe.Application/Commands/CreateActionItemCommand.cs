using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Commands;

public sealed record CreateActionItemCommand(
    string SessionId, string Description, string? AssigneeId, DateTime? DueDate, string RequestedBy)
    : IRequest<Result<ActionItemDto>>;

public sealed class CreateActionItemCommandHandler(
    IActionItemRepository actionItems, ISessionRepository sessions, IUserRepository users)
    : IRequestHandler<CreateActionItemCommand, Result<ActionItemDto>>
{
    public async Task<Result<ActionItemDto>> Handle(CreateActionItemCommand request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return Result<ActionItemDto>.Fail(DomainFailure.NotFound($"Session {request.SessionId} not found"));

        var requester = await users.FindByIdAsync(request.RequestedBy, ct);
        if (requester is null) return Result<ActionItemDto>.Fail(DomainFailure.Unauthorized("Usuário autenticado não encontrado"));

        var access = SquadAccessGuard.AssertSquadAccess(requester, session.SquadId);
        if (!access.IsOk) return Result<ActionItemDto>.FromFailure(access);

        var created = ActionItem.Create(Guid.NewGuid().ToString(), session.Id, request.Description, request.AssigneeId, request.DueDate);
        if (!created.IsOk) return Result<ActionItemDto>.FromFailure(created);

        var item = created.Value!;
        await actionItems.SaveAsync(item, ct);

        var assignee = item.AssigneeId is null ? null : await users.FindByIdAsync(item.AssigneeId, ct);
        return Result<ActionItemDto>.Ok(ActionItemAssembler.ToActionItemDto(item, assignee));
    }
}
