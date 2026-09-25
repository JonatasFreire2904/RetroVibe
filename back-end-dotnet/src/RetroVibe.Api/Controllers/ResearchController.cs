using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Api.Auth;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Api.Controllers;

[ApiController]
[Route("api/research")]
[Authorize(Policy = "Staff")]
public sealed class ResearchController(RetroVibeDbContext db) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] string? mode, [FromQuery] string? squadId, CancellationToken ct)
    {
        if (!ValidMode(mode)) return BadRequest(new { error = "VALIDATION", message = "Filtro de teste inválido" });
        var account = await db.Users.FirstAsync(u => u.Id == User.GetUserId(), ct);
        if (squadId is not null && !account.CanAccessSquad(squadId)) return Forbid();
        var sessions = await db.Sessions.Where(s => s.SurveyEnabled && s.Status == SessionStatus.Completed)
            .ToListAsync(ct);
        sessions = sessions.Where(s => account.CanAccessSquad(s.SquadId) &&
            (squadId is null || s.SquadId == squadId) && IncludeMode(s, mode)).ToList();
        var ids = sessions.Select(s => s.Id).ToList();
        var responses = await db.SurveyResponses.Where(r => ids.Contains(r.SessionId)).ToListAsync(ct);
        var guestCounts = await db.Users.Where(u => u.AllowedSessionId != null && ids.Contains(u.AllowedSessionId))
            .GroupBy(u => u.AllowedSessionId).Select(g => new { SessionId = g.Key, Count = g.Count() }).ToListAsync(ct);
        var actionItems = await db.ActionItems.CountAsync(a => ids.Contains(a.SessionId), ct);
        var expected = sessions.Count + guestCounts.Sum(g => g.Count);
        return Ok(new
        {
            sessions = sessions.Count, expectedResponses = expected, receivedResponses = responses.Count,
            completionPercent = expected == 0 ? 0 : Math.Round(100.0 * responses.Count / expected, 1),
            engagementAverage = Average(responses.Select(r => r.EngagementScore)),
            usabilityAverage = Average(responses.Select(r => r.UsabilityScore)),
            cards = sessions.Sum(s => s.Columns.Sum(c => c.Cards.Count)),
            votes = sessions.Sum(s => s.Columns.Sum(c => c.Cards.Sum(card => card.VoteCount))),
            actionItems,
            facilitator = SummarizeRole(responses, "FACILITATOR"),
            participants = SummarizeRole(responses, "PARTICIPANT"),
            scoreDistribution = Enumerable.Range(1, 5).Select(score => new
            {
                score, engagement = responses.Count(r => r.EngagementScore == score),
                usability = responses.Count(r => r.UsabilityScore == score)
            }),
            suggestions = responses.Where(r => !string.IsNullOrWhiteSpace(r.Suggestion))
                .Select(r => new { r.RespondentRole, r.Suggestion })
        });
    }

    [HttpGet("facilitators/{facilitatorId}/squads")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> FacilitatorSquads(string facilitatorId, [FromQuery] string? mode, CancellationToken ct)
    {
        if (!ValidMode(mode)) return BadRequest();
        var facilitator = await db.Users.FirstOrDefaultAsync(u => u.Id == facilitatorId && u.AccessLevel == AccessLevel.Facilitator, ct);
        if (facilitator is null) return NotFound();
        var squads = await db.Squads.ToListAsync(ct);
        var sessions = await db.Sessions.Where(s => s.FacilitatorId == facilitatorId && s.SurveyEnabled).ToListAsync(ct);
        return Ok(new { facilitator = new { facilitator.Id, facilitator.Name, facilitator.IsTest },
            squads = squads.Where(s => facilitator.CanAccessSquad(s.Id)).Select(s => new
            {
                s.Id, s.Name,
                sessions = sessions.Count(x => x.SquadId == s.Id && IncludeMode(x, mode))
            }) });
    }

    [HttpGet("squads/{squadId}/sessions")]
    public async Task<IActionResult> SquadSessions(string squadId, [FromQuery] string? mode,
        [FromQuery] string? facilitatorId, CancellationToken ct)
    {
        if (!ValidMode(mode)) return BadRequest();
        var account = await db.Users.FirstAsync(u => u.Id == User.GetUserId(), ct);
        if (!account.CanAccessSquad(squadId)) return Forbid();
        var sessions = await db.Sessions.Where(s => s.SquadId == squadId && s.SurveyEnabled)
            .OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
        sessions = sessions.Where(s => IncludeMode(s, mode) && (facilitatorId is null || s.FacilitatorId == facilitatorId)).ToList();
        var ids = sessions.Select(s => s.Id).ToList();
        var responses = await db.SurveyResponses.Where(r => ids.Contains(r.SessionId)).ToListAsync(ct);
        var guests = await db.Users.Where(u => u.AllowedSessionId != null && ids.Contains(u.AllowedSessionId)).ToListAsync(ct);
        return Ok(sessions.Select(s => new
        {
            s.Id, s.Title, s.Status, s.CreatedAt, s.ClosedAt, s.IsTest, s.FacilitatorId,
            expectedResponses = 1 + guests.Count(g => g.AllowedSessionId == s.Id),
            receivedResponses = responses.Count(r => r.SessionId == s.Id)
        }));
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> Session(string sessionId, CancellationToken ct)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.SurveyEnabled, ct);
        if (session is null) return NotFound();
        var account = await db.Users.FirstAsync(u => u.Id == User.GetUserId(), ct);
        if (!account.CanAccessSquad(session.SquadId)) return Forbid();
        var isAdmin = account.IsAdmin;
        var squad = await db.Squads.FirstAsync(s => s.Id == session.SquadId, ct);
        var facilitator = session.FacilitatorId is null ? null : await db.Users.FirstOrDefaultAsync(u => u.Id == session.FacilitatorId, ct);
        var guests = await db.Users.Where(u => u.AllowedSessionId == sessionId).OrderBy(u => u.Name).ToListAsync(ct);
        var responses = await db.SurveyResponses.Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.SubmittedAt).ToListAsync(ct);
        var actionItems = await db.ActionItems.CountAsync(a => a.SessionId == sessionId, ct);
        var respondents = new List<object>();
        if (facilitator is not null)
            respondents.Add(new { facilitator.Name, role = "FACILITATOR", completed = responses.Any(r => r.RespondentId == facilitator.Id) });
        respondents.AddRange(guests.Select(g => (object)new { g.Name, role = "PARTICIPANT",
            completed = responses.Any(r => r.RespondentId == g.Id) }));
        return Ok(new
        {
            session.Id, session.Title, session.CreatedAt, session.ClosedAt, session.Status, session.IsTest,
            squad = new { squad.Id, squad.Name },
            facilitator = facilitator is null ? null : new { facilitator.Id, facilitator.Name },
            expectedResponses = respondents.Count, receivedResponses = responses.Count,
            engagementAverage = Average(responses.Select(r => r.EngagementScore)),
            usabilityAverage = Average(responses.Select(r => r.UsabilityScore)),
            metrics = new
            {
                participants = respondents.Count,
                cards = session.Columns.Sum(c => c.Cards.Count),
                votes = session.Columns.Sum(c => c.Cards.Sum(card => card.VoteCount)),
                actionItems,
                durationMinutes = session.ClosedAt is null ? 0 : Math.Max(0, Math.Round((session.ClosedAt.Value - session.CreatedAt).TotalMinutes, 1))
            },
            respondents = isAdmin ? respondents : null,
            responses = isAdmin ? responses.Select((r, index) => new
            {
                number = index + 1, r.RespondentRole, r.EngagementScore, r.UsabilityScore,
                r.Suggestion
            }).ToList() : null
        });
    }

    private static bool ValidMode(string? mode) => mode is null or "real" or "test" or "all";
    private static bool IncludeMode(RetroSession session, string? mode) =>
        mode switch { "test" => session.IsTest, "all" => true, _ => !session.IsTest };
    private static double Average(IEnumerable<int> scores)
    {
        var values = scores.ToList();
        return values.Count == 0 ? 0 : Math.Round(values.Average(), 1);
    }
    private static object SummarizeRole(IEnumerable<SurveyResponseRow> all, string role)
    {
        var responses = all.Where(r => r.RespondentRole == role).ToList();
        return new { count = responses.Count, engagementAverage = Average(responses.Select(r => r.EngagementScore)),
            usabilityAverage = Average(responses.Select(r => r.UsabilityScore)) };
    }
}
