using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record SearchResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] SearchData Data
);
