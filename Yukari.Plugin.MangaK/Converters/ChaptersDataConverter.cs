using System.Text.Json;
using System.Text.Json.Serialization;
using Yukari.Plugin.MangaK.Data;

namespace Yukari.Plugin.MangaK.Converters;

internal class ChaptersDataConverter : JsonConverter<ChaptersData>
{
    public override ChaptersData Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            var chapters = JsonSerializer.Deserialize<MangaKChapter[]>(root.GetRawText(), options);
            return new ChaptersData(chapters ?? Array.Empty<MangaKChapter>());
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, MangaKChapter[]>>(
                root.GetRawText(),
                options
            );
            var allChapters =
                dictionary?.SelectMany(kvp => kvp.Value).ToArray() ?? Array.Empty<MangaKChapter>();
            return new ChaptersData(allChapters);
        }

        throw new JsonException("Unexpected format for chapters.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        ChaptersData value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value.Chapters, options);
    }
}
