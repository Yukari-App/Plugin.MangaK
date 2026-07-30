using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record ChaptersResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] ChaptersData Data
);
