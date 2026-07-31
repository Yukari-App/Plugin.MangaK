using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record MangaKChapter(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("name")] string Title,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("images")] string[] Pages
);
