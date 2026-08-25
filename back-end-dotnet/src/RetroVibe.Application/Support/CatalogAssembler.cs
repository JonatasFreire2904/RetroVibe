using RetroVibe.Application.Dtos;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Application.Support;

public static class CatalogAssembler
{
    public static TemplateDto ToTemplateDto(RetroTemplate template) => new(
        template.Id, template.Key, template.Label, template.Icon, template.Description,
        template.IsCustom, template.Active,
        template.Columns.Select(c => new TemplateColumnDto(c.Key, c.Label, c.Icon)).ToList());

    public static ThemeDto ToThemeDto(Theme theme) => new(theme.Id, theme.Key, theme.Label, theme.Emoji, theme.Active);

    public static SquadDto ToSquadDto(Squad squad) => new(squad.Id, squad.Name);
}
