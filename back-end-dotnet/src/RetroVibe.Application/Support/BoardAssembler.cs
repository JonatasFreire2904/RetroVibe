using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;
using RetroVibe.Domain.Kernel;
using RetroVibe.Domain.Repositories;

namespace RetroVibe.Application.Support;

public static class BoardAssembler
{
    public static async Task<Result<SessionBoardDto>> AssembleBoardAfterMutation(
        RetroSession session, string viewerId, ICatalogRepository catalog, IActionItemRepository actionItems, CancellationToken ct)
    {
        var templateTask = catalog.FindTemplateByIdAsync(session.TemplateId, ct);
        var themeTask = catalog.FindThemeByIdAsync(session.ThemeId, ct);
        var squadTask = catalog.FindSquadByIdAsync(session.SquadId, ct);
        var itemsTask = actionItems.FindAllAsync(new ActionItemListFilters(SessionId: session.Id), ct);
        await Task.WhenAll(templateTask, themeTask, squadTask, itemsTask);

        var template = await templateTask;
        var theme = await themeTask;
        var squad = await squadTask;
        var items = await itemsTask;

        if (template is null || theme is null || squad is null)
        {
            return Result<SessionBoardDto>.Fail(DomainFailure.NotFound("Referências de catálogo da sessão não encontradas"));
        }

        return Result<SessionBoardDto>.Ok(SessionAssembler.ToSessionBoardDto(
            session, CatalogAssembler.ToTemplateDto(template), CatalogAssembler.ToThemeDto(theme), squad, items.Count, viewerId));
    }
}
