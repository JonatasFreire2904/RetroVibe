using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record ListActionItemSessionsSummaryQuery(string? SquadId, string? TemplateId, string? ThemeId)
    : IRequest<List<ActionItemSessionSummaryDto>>;

public sealed class ListActionItemSessionsSummaryQueryHandler(
    ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems)
    : IRequestHandler<ListActionItemSessionsSummaryQuery, List<ActionItemSessionSummaryDto>>
{
    public async Task<List<ActionItemSessionSummaryDto>> Handle(ListActionItemSessionsSummaryQuery request, CancellationToken ct)
    {
        var filters = new SessionListFilters(request.SquadId, request.TemplateId, request.ThemeId);
        var allSessions = await sessions.FindAllAsync(filters, ct);
        var summaries = new List<ActionItemSessionSummaryDto>();

        foreach (var session in allSessions)
        {
            var items = await actionItems.FindAllAsync(new ActionItemListFilters(SessionId: session.Id), ct);
            if (items.Count == 0) continue;

            var template = await catalog.FindTemplateByIdAsync(session.TemplateId, ct);
            if (template is null) continue;

            var completed = items.Count(i => i.Status == ActionItemStatus.Done);
            summaries.Add(new ActionItemSessionSummaryDto(
                session.Id, template.Label, template.Icon, session.ClosedAt ?? session.CreatedAt, items.Count, completed));
        }

        return summaries.OrderByDescending(s => s.Date).ToList();
    }
}
