using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record ChapterDetailsResponse(
    [property: JsonPropertyName("success")] bool success,
    [property: JsonPropertyName("data")] ChapterDetailsData Data
);
