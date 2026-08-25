using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Infrastructure.Repositories;

public sealed class EfSessionRepository(RetroVibeDbContext db) : ISessionRepository
{
    public async Task SaveAsync(RetroSession session, CancellationToken ct = default)
    {
        if (db.Entry(session).State == EntityState.Detached)
        {
            var exists = await db.Sessions.AnyAsync(s => s.Id == session.Id, ct);
            if (exists) db.Sessions.Update(session);
            else db.Sessions.Add(session);
        }
        await db.SaveChangesAsync(ct);
    }

    public Task<RetroSession?> FindByIdAsync(string id, CancellationToken ct = default) =>
        db.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<RetroSession>> FindRecentAsync(int limit, string? squadId = null, CancellationToken ct = default)
    {
        var query = db.Sessions.AsQueryable();
        if (squadId is not null) query = query.Where(s => s.SquadId == squadId);
        return await query.OrderByDescending(s => s.CreatedAt).Take(limit).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RetroSession>> FindAllAsync(SessionListFilters filters, CancellationToken ct = default)
    {
        var query = db.Sessions.AsQueryable();
        if (filters.SquadId is not null) query = query.Where(s => s.SquadId == filters.SquadId);
        if (filters.TemplateId is not null) query = query.Where(s => s.TemplateId == filters.TemplateId);
        if (filters.ThemeId is not null) query = query.Where(s => s.ThemeId == filters.ThemeId);
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(s => s.Title != null && EF.Functions.Like(s.Title, $"%{term}%"));
        }
        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
    }
}
