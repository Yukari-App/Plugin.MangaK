using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record DetailsResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] DetailsData Data
);
