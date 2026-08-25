using RetroVibe.Domain.Entities;

namespace RetroVibe.Domain.Repositories;

public sealed record SessionListFilters(
    string? SquadId = null,
    string? TemplateId = null,
    string? ThemeId = null,
    string? Search = null);

public interface ISessionRepository
{
    Task SaveAsync(RetroSession session, CancellationToken ct = default);
    Task<RetroSession?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<RetroSession>> FindRecentAsync(int limit, string? squadId = null, CancellationToken ct = default);
    Task<IReadOnlyList<RetroSession>> FindAllAsync(SessionListFilters filters, CancellationToken ct = default);
}
