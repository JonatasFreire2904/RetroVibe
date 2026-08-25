using RetroVibe.Domain.Entities;

namespace RetroVibe.Domain.Repositories;

public sealed record CreateTemplateInput(string Key, string Label, string Icon, string Description, bool IsCustom, List<RetroColumnBlueprint> Columns);

public sealed record UpdateTemplateInput(
    string? Label = null, string? Icon = null, string? Description = null,
    bool? Active = null, List<RetroColumnBlueprint>? Columns = null);

public sealed record CreateThemeInput(string Key, string Label, string Emoji);

public sealed record UpdateThemeInput(string? Label = null, string? Emoji = null, bool? Active = null);

public interface ICatalogRepository
{
    Task<IReadOnlyList<RetroTemplate>> ListTemplatesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RetroTemplate>> ListAllTemplatesIncludingInactiveAsync(CancellationToken ct = default);
    Task<RetroTemplate?> FindTemplateByIdAsync(string id, CancellationToken ct = default);
    Task<RetroTemplate> CreateTemplateAsync(CreateTemplateInput input, CancellationToken ct = default);
    Task<RetroTemplate?> UpdateTemplateAsync(string id, UpdateTemplateInput patch, CancellationToken ct = default);

    Task<IReadOnlyList<Theme>> ListThemesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Theme>> ListAllThemesIncludingInactiveAsync(CancellationToken ct = default);
    Task<Theme?> FindThemeByIdAsync(string id, CancellationToken ct = default);
    Task<Theme> CreateThemeAsync(CreateThemeInput input, CancellationToken ct = default);
    Task<Theme?> UpdateThemeAsync(string id, UpdateThemeInput patch, CancellationToken ct = default);

    Task<IReadOnlyList<Squad>> ListSquadsAsync(CancellationToken ct = default);
    Task<Squad> FindOrCreateSquadByNameAsync(string name, CancellationToken ct = default);
    Task<Squad?> FindSquadByIdAsync(string id, CancellationToken ct = default);
}
