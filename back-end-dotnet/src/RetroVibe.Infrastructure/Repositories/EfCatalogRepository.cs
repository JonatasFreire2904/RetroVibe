using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;
using RetroVibe.Infrastructure.Persistence;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Infrastructure.Repositories;

public sealed class EfCatalogRepository(RetroVibeDbContext db) : ICatalogRepository
{
    private static RetroTemplate ToDomain(TemplateRow row) => new()
    {
        Id = row.Id,
        Key = row.Key,
        Label = row.Label,
        Icon = row.Icon,
        Description = row.Description,
        IsCustom = row.IsCustom,
        Active = row.Active,
        Columns = row.Columns
            .OrderBy(c => c.OrderIndex)
            .Select(c => new RetroColumnBlueprint(c.Key, c.Label, c.Icon))
            .ToList(),
    };

    private static Theme ToDomain(ThemeRow row) => new() { Id = row.Id, Key = row.Key, Label = row.Label, Emoji = row.Emoji, Active = row.Active };

    private static Squad ToDomain(SquadRow row) => new() { Id = row.Id, Name = row.Name };

    public async Task<IReadOnlyList<RetroTemplate>> ListTemplatesAsync(CancellationToken ct = default)
    {
        var rows = await db.Templates.Include(t => t.Columns).Where(t => t.Active).ToListAsync(ct);
        return rows.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<RetroTemplate>> ListAllTemplatesIncludingInactiveAsync(CancellationToken ct = default)
    {
        var rows = await db.Templates.Include(t => t.Columns).ToListAsync(ct);
        return rows.Select(ToDomain).ToList();
    }

    public async Task<RetroTemplate?> FindTemplateByIdAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Templates.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == id, ct);
        return row is null ? null : ToDomain(row);
    }

    public async Task<RetroTemplate> CreateTemplateAsync(CreateTemplateInput input, CancellationToken ct = default)
    {
        var row = new TemplateRow
        {
            Id = Guid.NewGuid().ToString(),
            Key = input.Key,
            Label = input.Label,
            Icon = input.Icon,
            Description = input.Description,
            IsCustom = input.IsCustom,
            Active = true,
            Columns = input.Columns.Select((c, index) => new TemplateColumnRow
            {
                Id = Guid.NewGuid().ToString(),
                TemplateId = "",
                Key = c.Key,
                Label = c.Label,
                Icon = c.Icon,
                OrderIndex = index,
            }).ToList(),
        };
        db.Templates.Add(row);
        await db.SaveChangesAsync(ct);
        return ToDomain(row);
    }

    public async Task<RetroTemplate?> UpdateTemplateAsync(string id, UpdateTemplateInput patch, CancellationToken ct = default)
    {
        var row = await db.Templates.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (row is null) return null;

        if (patch.Label is not null) row.Label = patch.Label;
        if (patch.Icon is not null) row.Icon = patch.Icon;
        if (patch.Description is not null) row.Description = patch.Description;
        if (patch.Active is not null) row.Active = patch.Active.Value;
        if (patch.Columns is not null)
        {
            db.TemplateColumns.RemoveRange(row.Columns);
            row.Columns = patch.Columns.Select((c, index) => new TemplateColumnRow
            {
                Id = Guid.NewGuid().ToString(),
                TemplateId = row.Id,
                Key = c.Key,
                Label = c.Label,
                Icon = c.Icon,
                OrderIndex = index,
            }).ToList();
        }

        await db.SaveChangesAsync(ct);
        return ToDomain(row);
    }

    public async Task<IReadOnlyList<Theme>> ListThemesAsync(CancellationToken ct = default) =>
        (await db.Themes.Where(t => t.Active).ToListAsync(ct)).Select(ToDomain).ToList();

    public async Task<IReadOnlyList<Theme>> ListAllThemesIncludingInactiveAsync(CancellationToken ct = default) =>
        (await db.Themes.ToListAsync(ct)).Select(ToDomain).ToList();

    public async Task<Theme?> FindThemeByIdAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Themes.FirstOrDefaultAsync(t => t.Id == id, ct);
        return row is null ? null : ToDomain(row);
    }

    public async Task<Theme> CreateThemeAsync(CreateThemeInput input, CancellationToken ct = default)
    {
        var row = new ThemeRow { Id = Guid.NewGuid().ToString(), Key = input.Key, Label = input.Label, Emoji = input.Emoji, Active = true };
        db.Themes.Add(row);
        await db.SaveChangesAsync(ct);
        return ToDomain(row);
    }

    public async Task<Theme?> UpdateThemeAsync(string id, UpdateThemeInput patch, CancellationToken ct = default)
    {
        var row = await db.Themes.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (row is null) return null;

        if (patch.Label is not null) row.Label = patch.Label;
        if (patch.Emoji is not null) row.Emoji = patch.Emoji;
        if (patch.Active is not null) row.Active = patch.Active.Value;

        await db.SaveChangesAsync(ct);
        return ToDomain(row);
    }

    public async Task<IReadOnlyList<Squad>> ListSquadsAsync(CancellationToken ct = default) =>
        (await db.Squads.ToListAsync(ct)).Select(ToDomain).ToList();

    public async Task<Squad> FindOrCreateSquadByNameAsync(string name, CancellationToken ct = default)
    {
        var row = await db.Squads.FirstOrDefaultAsync(s => s.Name == name, ct);
        if (row is not null) return ToDomain(row);

        row = new SquadRow { Id = Guid.NewGuid().ToString(), Name = name };
        db.Squads.Add(row);
        await db.SaveChangesAsync(ct);
        return ToDomain(row);
    }

    public async Task<Squad?> FindSquadByIdAsync(string id, CancellationToken ct = default)
    {
        var row = await db.Squads.FirstOrDefaultAsync(s => s.Id == id, ct);
        return row is null ? null : ToDomain(row);
    }
}
