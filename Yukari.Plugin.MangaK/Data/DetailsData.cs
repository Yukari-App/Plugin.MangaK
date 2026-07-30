using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record DetailsData([property: JsonPropertyName("title")] MangaKComic Comic);
