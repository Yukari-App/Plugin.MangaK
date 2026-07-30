using System.Text.Json.Serialization;
using Yukari.Plugin.MangaK.Converters;

namespace Yukari.Plugin.MangaK.Data;

[JsonConverter(typeof(ChaptersDataConverter))]
internal record ChaptersData([property: JsonPropertyName("chapters")] MangaKChapter[] Chapters);
