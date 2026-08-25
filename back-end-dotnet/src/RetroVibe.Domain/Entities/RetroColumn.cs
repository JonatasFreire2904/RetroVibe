namespace RetroVibe.Domain.Entities;

public sealed class RetroColumn
{
    private readonly List<RetroCard> _cards;

    public string Id { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Label { get; private set; } = null!;
    public string Icon { get; private set; } = null!;
    public int Order { get; private set; }
    public IReadOnlyList<RetroCard> Cards => _cards;

    private RetroColumn()
    {
        _cards = new List<RetroCard>();
    }

    public static RetroColumn Create(string id, string key, string label, string icon, int order)
    {
        return new RetroColumn { Id = id, Key = key, Label = label, Icon = icon, Order = order };
    }

    public static RetroColumn Restore(string id, string key, string label, string icon, int order, IEnumerable<RetroCard> cards)
    {
        var column = new RetroColumn { Id = id, Key = key, Label = label, Icon = icon, Order = order };
        column._cards.AddRange(cards);
        return column;
    }

    public void AddCard(RetroCard card) => _cards.Add(card);

    public RetroCard? FindCard(string cardId) => _cards.FirstOrDefault(c => c.Id == cardId);
}
