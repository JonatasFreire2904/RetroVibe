using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Application.Ports;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Api.Controllers;

public sealed record CreateFacilitatorRequest(string Name, string Username, string Password, bool IsTest);
public sealed record ResetFacilitatorPasswordRequest(string Password);
public sealed record UpdateFacilitatorTestRequest(bool IsTest);

[ApiController]
[Route("api/admin/facilitators")]
[Authorize(Policy = "Admin")]
public sealed class AdminFacilitatorsController(RetroVibeDbContext db, IPasswordHasher hasher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var facilitators = await db.Users.Where(u => u.AccessLevel == AccessLevel.Facilitator && u.Username != null)
            .OrderBy(u => u.Name).ToListAsync(ct);
        var squads = await db.Squads.ToListAsync(ct);
        return Ok(facilitators.Select(f => Present(f, squads)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFacilitatorRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        var username = request.Username?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || username.Length > 60 ||
            string.IsNullOrEmpty(request.Password) || request.Password.Length < 12)
            return BadRequest(new { error = "VALIDATION", message = "Informe nome, usuário e senha de pelo menos 12 caracteres" });
        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            return Conflict(new { error = "USERNAME_EXISTS", message = "Este usuário já existe" });

        var facilitator = RetroVibe.Domain.Entities.User.Restore(Guid.NewGuid().ToString(), name, "Facilitador", null, "#7C3AED",
            username, hasher.Hash(request.Password!), AccessLevel.Facilitator, null, isTest: request.IsTest);
        db.Users.Add(facilitator);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/facilitators/{facilitator.Id}", Present(facilitator, []));
    }

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetFacilitatorPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 12)
            return BadRequest(new { error = "VALIDATION", message = "A senha deve ter pelo menos 12 caracteres" });
        var facilitator = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.AccessLevel == AccessLevel.Facilitator, ct);
        if (facilitator is null) return NotFound();
        facilitator.ResetPassword(hasher.Hash(request.Password!));
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{id}/test")]
    public async Task<IActionResult> SetTest(string id, [FromBody] UpdateFacilitatorTestRequest request, CancellationToken ct)
    {
        var facilitator = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.AccessLevel == AccessLevel.Facilitator, ct);
        if (facilitator is null) return NotFound();
        facilitator.SetTest(request.IsTest);
        await db.SaveChangesAsync(ct);
        return Ok(Present(facilitator, await db.Squads.ToListAsync(ct)));
    }

    private static object Present(User user, IEnumerable<RetroVibe.Infrastructure.Persistence.Rows.SquadRow> squads) => new
    {
        user.Id, user.Name, user.Username, user.IsTest,
        Squads = squads.Where(s => user.CanAccessSquad(s.Id)).Select(s => new { s.Id, s.Name })
    };
}
