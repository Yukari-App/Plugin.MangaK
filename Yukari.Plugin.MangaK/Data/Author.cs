using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record Author([property: JsonPropertyName("name")] string Name);
