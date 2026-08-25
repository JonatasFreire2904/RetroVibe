using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Infrastructure.Repositories;

public sealed class EfActionItemRepository(RetroVibeDbContext db) : IActionItemRepository
{
    public async Task SaveAsync(ActionItem item, CancellationToken ct = default)
    {
        if (db.Entry(item).State == EntityState.Detached)
        {
            var exists = await db.ActionItems.AnyAsync(a => a.Id == item.Id, ct);
            if (exists) db.ActionItems.Update(item);
            else db.ActionItems.Add(item);
        }
        await db.SaveChangesAsync(ct);
    }

    public Task<ActionItem?> FindByIdAsync(string id, CancellationToken ct = default) =>
        db.ActionItems.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<ActionItem>> FindAllAsync(ActionItemListFilters filters, CancellationToken ct = default)
    {
        var query = db.ActionItems.AsQueryable();

        if (filters.SessionId is not null) query = query.Where(a => a.SessionId == filters.SessionId);
        if (filters.Status is not null) query = query.Where(a => a.Status == filters.Status);

        if (filters.SquadId is not null || filters.TemplateId is not null || filters.ThemeId is not null)
        {
            var sessionIds = db.Sessions.AsQueryable();
            if (filters.SquadId is not null) sessionIds = sessionIds.Where(s => s.SquadId == filters.SquadId);
            if (filters.TemplateId is not null) sessionIds = sessionIds.Where(s => s.TemplateId == filters.TemplateId);
            if (filters.ThemeId is not null) sessionIds = sessionIds.Where(s => s.ThemeId == filters.ThemeId);
            var matchingSessionIds = sessionIds.Select(s => s.Id);
            query = query.Where(a => matchingSessionIds.Contains(a.SessionId));
        }

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }
}
