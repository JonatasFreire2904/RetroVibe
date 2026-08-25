using MediatR;
using RetroVibe.Application.Dtos;
using RetroVibe.Application.Support;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Queries;

public sealed record GetSessionBoardQuery(string SessionId, string ViewerId) : IRequest<SessionBoardDto?>;

public sealed class GetSessionBoardQueryHandler(ISessionRepository sessions, ICatalogRepository catalog, IActionItemRepository actionItems)
    : IRequestHandler<GetSessionBoardQuery, SessionBoardDto?>
{
    public async Task<SessionBoardDto?> Handle(GetSessionBoardQuery request, CancellationToken ct)
    {
        var session = await sessions.FindByIdAsync(request.SessionId, ct);
        if (session is null) return null;

        var templateTask = catalog.FindTemplateByIdAsync(session.TemplateId, ct);
        var themeTask = catalog.FindThemeByIdAsync(session.ThemeId, ct);
        var squadTask = catalog.FindSquadByIdAsync(session.SquadId, ct);
        var itemsTask = actionItems.FindAllAsync(new ActionItemListFilters(SessionId: session.Id), ct);
        await Task.WhenAll(templateTask, themeTask, squadTask, itemsTask);

        var template = await templateTask;
        var theme = await themeTask;
        var squad = await squadTask;
        var items = await itemsTask;
        if (template is null || theme is null || squad is null) return null;

        return SessionAssembler.ToSessionBoardDto(
            session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, items.Count, request.ViewerId);
    }
}
