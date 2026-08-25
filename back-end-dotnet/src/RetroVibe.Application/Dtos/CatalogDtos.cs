namespace RetroVibe.Application.Dtos;

public sealed record TemplateColumnDto(string Key, string Label, string Icon);

public sealed record TemplateDto(
    string Id, string Key, string Label, string Icon, string Description,
    bool IsCustom, bool Active, List<TemplateColumnDto> Columns);

public sealed record ThemeDto(string Id, string Key, string Label, string Emoji, bool Active);

public sealed record SquadDto(string Id, string Name);
