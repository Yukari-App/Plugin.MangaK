using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record MangaKComic(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("name")] string Title,
    [property: JsonPropertyName("authors")] Author[] Authors,
    [property: JsonPropertyName("summary")] string Description,
    [property: JsonPropertyName("genres")] Genre[] Genres,
    [property: JsonPropertyName("release_date")] string Year,
    [property: JsonPropertyName("cover")] string CoverUrl,
    [property: JsonPropertyName("status")] string Status
);
