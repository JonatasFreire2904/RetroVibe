using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Api.Auth;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Api.Controllers;

public sealed record CreateSquadRequest(string Name);

[ApiController]
[Route("api/squads")]
[Authorize(Policy = "Staff")]
public sealed class SquadsController(RetroVibeDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSquadRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80)
            return BadRequest(new { error = "VALIDATION", message = "Informe um nome de squad de até 80 caracteres" });
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId(), ct);
        if (user is null || user.AccessLevel != AccessLevel.Facilitator) return Forbid();
        if (await db.Squads.AnyAsync(s => s.Name.ToLower() == name.ToLower(), ct))
            return Conflict(new { error = "SQUAD_EXISTS", message = "Já existe um squad com este nome" });
        var squad = new SquadRow { Id = Guid.NewGuid().ToString(), Name = name };
        db.Squads.Add(squad);
        user.AddManagedSquad(squad.Id);
        await db.SaveChangesAsync(ct);
        return Created($"/api/squads/{squad.Id}", new { squad.Id, squad.Name });
    }
}
