using System.Text.Json.Serialization;

namespace Yukari.Plugin.MangaK.Data;

internal record ChapterDetailsData([property: JsonPropertyName("chapter")] MangaKChapter Chapter);
