namespace RetroVibe.Domain.Entities;

public sealed record RetroColumnBlueprint(string Key, string Label, string Icon);

public sealed class RetroTemplate
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Label { get; init; }
    public required string Icon { get; init; }
    public required string Description { get; init; }
    public required bool IsCustom { get; init; }
    public required bool Active { get; set; }
    public required List<RetroColumnBlueprint> Columns { get; init; }
}

public sealed class Theme
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Label { get; init; }
    public required string Emoji { get; init; }
    public required bool Active { get; set; }
}

public sealed class Squad
{
    public required string Id { get; init; }
    public required string Name { get; init; }
}
