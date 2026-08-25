using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListSessionsQuery(string? SquadId, string? TemplateId, string? ThemeId, string? Search)
    : IRequest<List<SessionSummaryDto>>;

public sealed class ListSessionsQueryHandler(ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems)
    : IRequestHandler<ListSessionsQuery, List<SessionSummaryDto>>
{
    public async Task<List<SessionSummaryDto>> Handle(ListSessionsQuery request, CancellationToken ct)
    {
        var filters = new SessionListFilters(request.SquadId, request.TemplateId, request.ThemeId, request.Search);
        var all = await sessions.FindAllAsync(filters, ct);
        var summaries = new List<SessionSummaryDto>();

        foreach (var session in all)
        {
            var template = await catalog.FindTemplateByIdAsync(session.TemplateId, ct);
            var theme = await catalog.FindThemeByIdAsync(session.ThemeId, ct);
            var squad = await catalog.FindSquadByIdAsync(session.SquadId, ct);
            if (template is null || theme is null || squad is null) continue;

            var items = await actionItems.FindAllAsync(new ActionItemListFilters(SessionId: session.Id), ct);
            summaries.Add(SessionAssembler.ToSessionSummaryDto(
                session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, items.Count));
        }

        return summaries;
    }
}
