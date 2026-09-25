using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Api.Auth;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Api.Controllers;

public sealed record SubmitSurveyRequest(int EngagementScore, int UsabilityScore, string? Suggestion);

[ApiController]
[Route("api/sessions/{sessionId}/survey")]
[Authorize]
public sealed class SurveyController(RetroVibeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Status(string sessionId, CancellationToken ct)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        if (session is null) return NotFound();
        var role = ResponseRole(session, User.GetUserId(), User.GetAccessLevel(), User.GetAllowedSessionId());
        if (role is null) return Forbid();
        var completed = await db.SurveyResponses.AnyAsync(r => r.SessionId == sessionId && r.RespondentId == User.GetUserId(), ct);
        return Ok(new { enabled = session.SurveyEnabled, open = session.Status == SessionStatus.Completed,
            completed, role });
    }

    [HttpPost]
    public async Task<IActionResult> Submit(string sessionId, [FromBody] SubmitSurveyRequest request, CancellationToken ct)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        if (session is null) return NotFound();
        var role = ResponseRole(session, User.GetUserId(), User.GetAccessLevel(), User.GetAllowedSessionId());
        if (role is null) return Forbid();
        if (!session.SurveyEnabled || session.Status != SessionStatus.Completed)
            return Conflict(new { error = "SURVEY_NOT_OPEN", message = "O questionário abre após o encerramento da sessão" });
        if (request.EngagementScore is < 1 or > 5 || request.UsabilityScore is < 1 or > 5 ||
            request.Suggestion?.Length > 2000)
            return BadRequest(new { error = "VALIDATION", message = "As notas devem estar entre 1 e 5; a sugestão deve ter até 2000 caracteres" });
        if (await db.SurveyResponses.AnyAsync(r => r.SessionId == sessionId && r.RespondentId == User.GetUserId(), ct))
            return Conflict(new { error = "ALREADY_SUBMITTED", message = "Você já respondeu este questionário" });

        var response = new SurveyResponseRow
        {
            Id = Guid.NewGuid().ToString(), SessionId = sessionId, RespondentId = User.GetUserId(),
            RespondentRole = role, EngagementScore = request.EngagementScore, UsabilityScore = request.UsabilityScore,
            Suggestion = request.Suggestion?.Trim() ?? "", SubmittedAt = DateTime.UtcNow
        };
        db.SurveyResponses.Add(response);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict(new { error = "ALREADY_SUBMITTED", message = "Você já respondeu este questionário" }); }
        return StatusCode(StatusCodes.Status201Created, new { completed = true });
    }

    private static string? ResponseRole(RetroSession session, string userId, AccessLevel accessLevel, string? allowedSessionId)
    {
        if (session.FacilitatorId == userId) return "FACILITATOR";
        if (accessLevel == AccessLevel.Participant && allowedSessionId == session.Id) return "PARTICIPANT";
        return null;
    }
}
