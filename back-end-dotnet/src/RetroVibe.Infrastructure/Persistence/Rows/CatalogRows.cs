namespace RetroVibe.Infrastructure.Persistence.Rows;

public sealed class SquadRow
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public sealed class ThemeRow
{
    public string Id { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string Emoji { get; set; } = null!;
    public bool Active { get; set; }
}

public sealed class TemplateRow
{
    public string Id { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsCustom { get; set; }
    public bool Active { get; set; }
    public List<TemplateColumnRow> Columns { get; set; } = new();
}

public sealed class TemplateColumnRow
{
    public string Id { get; set; } = null!;
    public string TemplateId { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public int OrderIndex { get; set; }
}
