using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroVibe.Api.Auth;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Api.Controllers;

public sealed record CreateSquadRequest(string Name, List<string>? Members);
public sealed record AddSquadMemberRequest(string Name);

[ApiController]
[Route("api/squads")]
[Authorize(Policy = "Staff")]
public sealed class SquadsController(RetroVibeDbContext db) : ControllerBase
{
    private const int MaxNameLength = 80;
    private const int MaxMembers = 50;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSquadRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
            return BadRequest(new { error = "VALIDATION", message = "Informe um nome de squad de até 80 caracteres" });
        var memberNames = (request.Members ?? []).Select(m => m?.Trim() ?? "").Where(m => m.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (memberNames.Count > MaxMembers || memberNames.Any(m => m.Length > MaxNameLength))
            return BadRequest(new { error = "VALIDATION", message = "Informe até 50 membros com nomes de até 80 caracteres" });
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId(), ct);
        if (user is null || user.AccessLevel != AccessLevel.Facilitator) return Forbid();
        if (await db.Squads.AnyAsync(s => s.Name.ToLower() == name.ToLower(), ct))
            return Conflict(new { error = "SQUAD_EXISTS", message = "Já existe um squad com este nome" });
        var squad = new SquadRow { Id = Guid.NewGuid().ToString(), Name = name };
        db.Squads.Add(squad);
        user.AddManagedSquad(squad.Id);
        var members = memberNames.Select(memberName => NewMember(memberName, squad.Id)).ToList();
        db.Users.AddRange(members);
        await db.SaveChangesAsync(ct);
        return Created($"/api/squads/{squad.Id}", new { squad.Id, squad.Name, Members = members.Select(Present) });
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> ListMembers(string id, CancellationToken ct)
    {
        var denied = await EnsureSquadAccess(id, ct);
        if (denied is not null) return denied;
        return Ok((await Members(id, ct)).Select(Present));
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(string id, [FromBody] AddSquadMemberRequest request, CancellationToken ct)
    {
        var denied = await EnsureSquadAccess(id, ct);
        if (denied is not null) return denied;
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
            return BadRequest(new { error = "VALIDATION", message = "Informe um nome de membro de até 80 caracteres" });
        var members = await Members(id, ct);
        if (members.Count >= MaxMembers)
            return BadRequest(new { error = "VALIDATION", message = "O squad já tem o limite de 50 membros" });
        if (members.Any(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "MEMBER_EXISTS", message = "Este membro já está no squad" });
        var member = NewMember(name, id);
        db.Users.Add(member);
        await db.SaveChangesAsync(ct);
        return Created($"/api/squads/{id}/members/{member.Id}", Present(member));
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(string id, string memberId, CancellationToken ct)
    {
        var denied = await EnsureSquadAccess(id, ct);
        if (denied is not null) return denied;
        var member = await db.Users.FirstOrDefaultAsync(u => u.Id == memberId && u.SquadId == id && u.AccessLevel == AccessLevel.Member, ct);
        if (member is null) return NotFound();
        // Sai do squad mas o registro fica, para os itens de ação já atribuídos continuarem mostrando o nome.
        member.LeaveSquad();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<IActionResult?> EnsureSquadAccess(string squadId, CancellationToken ct)
    {
        if (!await db.Squads.AnyAsync(s => s.Id == squadId, ct)) return NotFound();
        var account = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId(), ct);
        return account?.CanAccessSquad(squadId) == true ? null : Forbid();
    }

    private async Task<List<User>> Members(string squadId, CancellationToken ct) =>
        await db.Users.Where(u => u.SquadId == squadId && u.AccessLevel == AccessLevel.Member)
            .OrderBy(u => u.Name).ToListAsync(ct);

    private static User NewMember(string name, string squadId)
    {
        var id = Guid.NewGuid().ToString();
        return RetroVibe.Domain.Entities.User.CreateSquadMember(id, name, squadId, AvatarColors.PickDeterministic(id));
    }

    private static object Present(User member) => new { member.Id, member.Name, member.AvatarColor };
}
