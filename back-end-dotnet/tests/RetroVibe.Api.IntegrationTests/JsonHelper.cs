using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroVibe.Api.IntegrationTests;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
