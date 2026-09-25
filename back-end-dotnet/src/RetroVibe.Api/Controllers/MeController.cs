using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Commands;
using RetroVibe.Application.Queries;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Api.Controllers;

public sealed record UpdateProfileRequest(string Name, string? Role, string AvatarColor);

[ApiController]
[Route("api/me")]
[Authorize(Policy = "Staff")]
public sealed class MeController(IMediator mediator, RetroVibeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var user = await mediator.Send(new GetCurrentUserQuery(User.GetUserId()), ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("pending-surveys")]
    public async Task<IActionResult> PendingSurveys(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var answered = db.SurveyResponses.Where(r => r.RespondentId == userId).Select(r => r.SessionId);
        var pending = await db.Sessions.Where(s => s.FacilitatorId == userId && s.SurveyEnabled &&
            s.Status == SessionStatus.Completed && !answered.Contains(s.Id))
            .OrderBy(s => s.ClosedAt).Select(s => new { sessionId = s.Id, title = s.Title }).ToListAsync(ct);
        return Ok(pending);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateUserProfileCommand(User.GetUserId(), request.Name, request.Role ?? "", request.AvatarColor), ct);
        return result.ToActionResult();
    }
}
