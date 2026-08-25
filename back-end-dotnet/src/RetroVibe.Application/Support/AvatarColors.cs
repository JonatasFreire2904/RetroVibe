namespace RetroVibe.Application.Support;

public static class AvatarColors
{
    public static readonly string[] Palette =
    [
        "#7C3AED", "#22C55E", "#F97316", "#0EA5E9",
        "#EC4899", "#EAB308", "#EF4444", "#14B8A6",
    ];

    public static string PickDeterministic(string seed)
    {
        var sum = seed.Sum(ch => (int)ch);
        return Palette[sum % Palette.Length];
    }
}
