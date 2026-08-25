using RetroVibe.Domain.Entities;

namespace RetroVibe.Domain.Repositories;

public sealed record ActionItemListFilters(
    string? SessionId = null,
    string? SquadId = null,
    string? TemplateId = null,
    string? ThemeId = null,
    ActionItemStatus? Status = null);

public interface IActionItemRepository
{
    Task SaveAsync(ActionItem item, CancellationToken ct = default);
    Task<ActionItem?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ActionItem>> FindAllAsync(ActionItemListFilters filters, CancellationToken ct = default);
}
