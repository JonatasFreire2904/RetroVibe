using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Queries;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Api.Controllers;

public sealed record JoinSessionRequest(string DisplayName);
public sealed record CreateSessionRequest(string? Title, string TemplateId, string? ThemeId, string SquadName,
    PrivacyMode? PrivacyMode, bool? SequentialFlow, bool? ActionCardsEnabled, bool? CardBlurEnabled);
public sealed record UpdateSessionRequest(string? Title, PrivacyMode PrivacyMode, bool SequentialFlow, bool ActionCardsEnabled,
    bool? CardBlurEnabled);
public sealed record NavigateStageRequest(SessionPhase Phase, int ActiveColumnIndex);
public sealed record CloseSessionRequest(double? FeedbackScore);
public sealed record AddCardRequest(string ColumnId, string Text);
public sealed record EditCardRequest(string Text);
public sealed record VoteRequest(string CardId);
public sealed record AddCommentRequest(string CardId, string Text);

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(IMediator mediator, ISessionRepository sessions, IUserRepository users) : ControllerBase
{
    [HttpPost("{id}/join")]
    [AllowAnonymous]
    public async Task<IActionResult> Join(string id, [FromBody] JoinSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new JoinSessionCommand(id, request.DisplayName), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpGet]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> ListSessions(
        [FromQuery] string? squadId, [FromQuery] string? templateId, [FromQuery] string? themeId, [FromQuery] string? search,
        CancellationToken ct)
    {
        var effectiveSquadId = SquadScope.EffectiveSquadId(User, squadId);
        var result = await mediator.Send(new ListSessionsQuery(effectiveSquadId, templateId, themeId, search), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateSessionCommand(request.Title, request.TemplateId, request.ThemeId, request.SquadName,
                request.PrivacyMode, request.SequentialFlow, request.ActionCardsEnabled, request.CardBlurEnabled, User.GetUserId()), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPatch("{id}/close")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> CloseSession(string id, [FromBody] CloseSessionRequest? request, CancellationToken ct)
    {
        var result = await mediator.Send(new CloseSessionCommand(id, request?.FeedbackScore, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/pause")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> PauseSession(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new PauseSessionCommand(id, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/resume")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> ResumeSession(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new ResumeSessionCommand(id, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/phase")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> AdvancePhase(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new AdvancePhaseCommand(id, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/stage")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> NavigateStage(string id, [FromBody] NavigateStageRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new NavigateStageCommand(id, request.Phase, request.ActiveColumnIndex, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/reveal-cards")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> RevealCards(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new RevealSessionCardsCommand(id, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/settings")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> UpdateSettings(string id, [FromBody] UpdateSessionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateSessionSettingsCommand(id, request.Title, request.PrivacyMode,
            request.SequentialFlow, request.ActionCardsEnabled, request.CardBlurEnabled, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(string id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var board = await mediator.Send(new GetSessionBoardQuery(id, userId), ct);
        if (board is null) return NotFound(new { error = "NOT_FOUND", message = $"Session {id} not found" });

        if (!IsAllowedOnSession(board.Squad.Id, board.Id))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "FORBIDDEN", message = "Você não tem acesso a esta sessão" });
        }

        return Ok(board);
    }

    [HttpGet("{id}/assignees")]
    [Authorize(Policy = "Staff")]
    public async Task<IActionResult> ListAssignees(string id, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(id, ct);
        if (session is null) return NotFound();
        if (!IsAllowedOnSession(session.SquadId, id)) return Forbid();
        var members = await users.ListBySquadAsync(session.SquadId, ct);
        return Ok(members.Select(member => new { member.Id, member.Name, member.AvatarColor }));
    }

    [HttpGet("{id}/action-items")]
    public async Task<IActionResult> ListSessionActionItems(string id, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(id, ct);
        if (session is null) return NotFound();
        if (!IsAllowedOnSession(session.SquadId, id)) return Forbid();
        var items = await mediator.Send(new ListActionItemsQuery(id, null, null, null, null), ct);
        return Ok(items);
    }

    [HttpPost("{id}/cards")]
    public async Task<IActionResult> AddCard(string id, [FromBody] AddCardRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddCardCommand(id, request.ColumnId, request.Text, User.GetUserId()), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPatch("{id}/cards/{cardId}")]
    public async Task<IActionResult> EditCard(string id, string cardId, [FromBody] EditCardRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new EditCardCommand(id, cardId, request.Text, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPost("{id}/votes")]
    public async Task<IActionResult> ToggleVote(string id, [FromBody] VoteRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ToggleVoteCommand(id, request.CardId, User.GetUserId()), ct);
        return result.ToActionResult();
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddCardCommentCommand(id, request.CardId, request.Text, User.GetUserId()), ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    [HttpGet("{id}/cards/{cardId}/comments")]
    public async Task<IActionResult> ListCardComments(string id, string cardId, CancellationToken ct)
    {
        var board = await mediator.Send(new GetSessionBoardQuery(id, User.GetUserId()), ct);
        if (board is null) return NotFound(new { error = "NOT_FOUND", message = $"Session {id} not found" });

        if (!IsAllowedOnSession(board.Squad.Id, board.Id))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "FORBIDDEN", message = "Você não tem acesso a esta sessão" });
        }

        var comments = await mediator.Send(new ListCardCommentsQuery(id, cardId), ct);
        return Ok(comments ?? []);
    }

    private bool IsAllowedOnSession(string boardSquadId, string boardSessionId)
    {
        var accessLevel = User.GetAccessLevel();
        return accessLevel == AccessLevel.Admin
            || (accessLevel == AccessLevel.Facilitator && boardSquadId == User.GetSquadId())
            || (accessLevel == AccessLevel.Participant && User.GetAllowedSessionId() == boardSessionId);
    }
}
