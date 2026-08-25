using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroVibe.Api.Json;

public static class EnumQueryParser
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };

    public static TEnum? Parse<TEnum>(string? raw) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(raw)) return null;
        return JsonSerializer.Deserialize<TEnum>($"\"{raw}\"", Options);
    }
}
