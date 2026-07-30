using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record Genre([property: JsonPropertyName("name")] string Name);
