using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record SearchData([property: JsonPropertyName("items")] MangaKComic[] Comics);
