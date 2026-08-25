using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Api.Json;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Queries;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Api.Controllers;

public sealed record CreateActionItemRequest(string SessionId, string Description, string? AssigneeId, DateTime? DueDate);

public sealed record UpdateActionItemRequest(
    ActionItemStatus? Status, string? Description, Optional<string?> AssigneeId, Optional<DateTime?> DueDate);

public sealed record AddActionItemCommentRequest(string Text);

[ApiController]
[Route("api/action-items")]
[Authorize(Policy = "Staff")]
public sealed class ActionItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListActionItems(
        [FromQuery] string? sessionId, [FromQuery] string? squadId, [FromQuery] string? templateId,
        [FromQuery] string? themeId, [FromQuery] string? status, CancellationToken ct)
    {
        var effectiveSquadId = SquadScope.EffectiveSquadId(User, squadId);
        var statusFilter = EnumQueryParser.Parse<ActionItemStatus>(status);
        var result = await mediator.Send(new ListActionItemsQuery(sessionId, effectiveSquadId, templateId, themeId, statusFilter), ct);
        return Ok(result);
    }

    [HttpGet("sessions-summary")]
    public async Task<IActionResult> ListSessionsSummary(
        [FromQuery] string? squadId, [FromQuery] string? templateId, [FromQuery] string? themeId, CancellationToken ct)
    {
        var effectiveSquadId = SquadScope.EffectiveSquadId(User, squadId);
        var result = await mediator.Send(new ListActionItemSessionsSummaryQuery(effectiveSquadId, templateId, themeId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateActionItem([FromBody] CreateActionItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateActionItemCommand(request.SessionId, request.Description, request.AssigneeId, request.DueDate, User.GetUserId()), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateActionItem(string id, [FromBody] UpdateActionItemRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateActionItemCommand(id, request.Status, request.Description, request.AssigneeId, request.DueDate, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> ListComments(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new ListActionItemCommentsQuery(id, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddActionItemCommentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddActionItemCommentCommand(id, request.Text, User.GetUserId()), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }
}
