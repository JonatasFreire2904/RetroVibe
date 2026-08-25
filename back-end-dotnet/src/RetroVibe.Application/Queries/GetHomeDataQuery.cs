using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record GetHomeDataQuery(string? SquadId) : IRequest<HomeDataDto>;

public sealed class GetHomeDataQueryHandler(ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems)
    : IRequestHandler<GetHomeDataQuery, HomeDataDto>
{
    public async Task<HomeDataDto> Handle(GetHomeDataQuery request, CancellationToken ct)
    {
        var recent = await sessions.FindRecentAsync(3, request.SquadId, ct);
        var summaries = new List<SessionSummaryDto>();

        foreach (var session in recent)
        {
            var template = await catalog.FindTemplateByIdAsync(session.TemplateId, ct);
            var theme = await catalog.FindThemeByIdAsync(session.ThemeId, ct);
            var squad = await catalog.FindSquadByIdAsync(session.SquadId, ct);
            if (template is null || theme is null || squad is null) continue;

            var items = await actionItems.FindAllAsync(new ActionItemListFilters(SessionId: session.Id), ct);
            summaries.Add(SessionAssembler.ToSessionSummaryDto(
                session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, items.Count));
        }

        var allTemplates = await catalog.ListTemplatesAsync(ct);
        var quickTemplates = allTemplates.Where(t => !t.IsCustom).Select(CatalogAssembler.ToTemplateDto).ToList();

        return new HomeDataDto(summaries, quickTemplates);
    }
}
