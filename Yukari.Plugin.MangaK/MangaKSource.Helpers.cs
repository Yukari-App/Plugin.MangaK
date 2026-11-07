using System.Net;
using System.Net.Http.Json;
using Yukari.Core.Models;

namespace Yukari.Plugin.MangaK;

public partial class MangaKSource
{
    private static readonly IReadOnlyList<Filter> _filters =
    [
        new Filter(
            Key: "content_rating",
            DisplayName: "Content Rating",
            Options:
            [
                new FilterOption("", "Any", true),
                new FilterOption("safe", "Safe", false),
                new FilterOption("suggestive", "Suggestive", false),
                new FilterOption("erotica", "Erotica", false),
                new FilterOption("pornographic", "Pornographic", false),
            ],
            AllowMultiple: false
        ),
        new Filter(
            Key: "type",
            DisplayName: "Type",
            Options:
            [
                new FilterOption("", "Any", true),
                new FilterOption("manga", "Manga", false),
                new FilterOption("manhwa", "Manhwa", false),
                new FilterOption("manhua", "Manhua", false),
            ],
            AllowMultiple: false
        ),
        new Filter(
            Key: "demographic",
            DisplayName: "Demographic",
            Options:
            [
                new FilterOption("", "Any", true),
                new FilterOption("shounen,seinen", "Boy (Shounen + Seinen)", false),
                new FilterOption("shoujo,josei", "Girl (Shoujo + Josei)", false),
                new FilterOption("shounen", "Shounen", false),
                new FilterOption("shoujo", "Shoujo", false),
                new FilterOption("seinen", "Seinen", false),
                new FilterOption("josei", "Josei", false),
            ],
            AllowMultiple: false
        ),
        new Filter(
            Key: "status",
            DisplayName: "Status",
            Options:
            [
                new FilterOption("", "Any", true),
                new FilterOption("ongoing", "Ongoing", false),
                new FilterOption("completed", "Completed", false),
                new FilterOption("hiatus", "Hiatus", false),
                new FilterOption("cancelled", "Cancelled", false),
            ],
            AllowMultiple: false
        ),
        new Filter(
            Key: "genres",
            DisplayName: "Genres",
            Options:
            [
                new FilterOption("action", "Action", false),
                new FilterOption("adaptation", "Adaptation", false),
                new FilterOption("adult", "Adult", false),
                new FilterOption("adventure", "Adventure", false),
                new FilterOption("anthology", "Anthology", false),
                new FilterOption("comedy", "Comedy", false),
                new FilterOption("comic", "Comic", false),
                new FilterOption("cooking", "Cooking", false),
                new FilterOption("demons", "Demons", false),
                new FilterOption("doujinshi", "Doujinshi", false),
                new FilterOption("drama", "Drama", false),
                new FilterOption("ecchi", "Ecchi", false),
                new FilterOption("fantasy", "Fantasy", false),
                new FilterOption("full-color", "Full Color", false),
                new FilterOption("game", "Game", false),
                new FilterOption("gender-bender", "Gender Bender", false),
                new FilterOption("ghosts", "Ghosts", false),
                new FilterOption("harem", "Harem", false),
                new FilterOption("hentai", "Hentai", false),
                new FilterOption("historical", "Historical", false),
                new FilterOption("horror", "Horror", false),
                new FilterOption("isekai", "Isekai", false),
                new FilterOption("josei", "Josei", false),
                new FilterOption("long-strip", "Long Strip", false),
                new FilterOption("magic", "Magic", false),
                new FilterOption("manga", "Manga", false),
                new FilterOption("manhua", "Manhua", false),
                new FilterOption("manhwa", "Manhwa", false),
                new FilterOption("martial-arts", "Martial Arts", false),
                new FilterOption("mature", "Mature", false),
                new FilterOption("mecha", "Mecha", false),
                new FilterOption("medical", "Medical", false),
                new FilterOption("military", "Military", false),
                new FilterOption("monster", "Monster", false),
                new FilterOption("monster-girls", "Monster Girls", false),
                new FilterOption("monsters", "Monsters", false),
                new FilterOption("music", "Music", false),
                new FilterOption("mystery", "Mystery", false),
                new FilterOption("office-workers", "Office Workers", false),
                new FilterOption("one-shot", "One Shot", false),
                new FilterOption("police", "Police", false),
                new FilterOption("psychological", "Psychological", false),
                new FilterOption("reincarnation", "Reincarnation", false),
                new FilterOption("romance", "Romance", false),
                new FilterOption("school-life", "School Life", false),
                new FilterOption("sci-fi", "Sci Fi", false),
                new FilterOption("science-fiction", "Science Fiction", false),
                new FilterOption("shoujo", "Shoujo", false),
                new FilterOption("shoujo-ai", "Shoujo Ai", false),
                new FilterOption("shounen", "Shounen", false),
                new FilterOption("shounen-ai", "Shounen Ai", false),
                new FilterOption("slice-of-life", "Slice of Life", false),
                new FilterOption("smut", "Smut", false),
                new FilterOption("soft-yaoi", "Soft Yaoi", false),
                new FilterOption("sports", "Sports", false),
                new FilterOption("super-power", "Super Power", false),
                new FilterOption("superhero", "Superhero", false),
                new FilterOption("supernatural", "Supernatural", false),
                new FilterOption("thriller", "Thriller", false),
                new FilterOption("time-travel", "Time Travel", false),
                new FilterOption("tragedy", "Tragedy", false),
                new FilterOption("vampire", "Vampire", false),
                new FilterOption("vampires", "Vampires", false),
                new FilterOption("video-games", "Video Games", false),
                new FilterOption("villainess", "Villainess", false),
                new FilterOption("web-comic", "Web Comic", false),
                new FilterOption("webtoons", "Webtoons", false),
                new FilterOption("worth-the-read", "Worth the Read", false),
                new FilterOption("yaoi", "Yaoi", false),
                new FilterOption("yuri", "Yuri", false),
                new FilterOption("zombies", "Zombies", false),
            ],
            AllowMultiple: true
        ),
    ];

    private static readonly IReadOnlyDictionary<string, string> _languages = new Dictionary<
        string,
        string
    >
    {
        { "en", "English" },
    };

    private async Task<T?> GetFromApiAsync<T>(string url, CancellationToken ct = default)
    {
        using var response = await _httpClient!.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, url),
            ct
        );

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new HttpRequestException(
                "MangaK Rate Limit Exceeded. Try again later.",
                null,
                HttpStatusCode.TooManyRequests
            );

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    private static ComicStatus GetComicStatus(string? status) =>
        status switch
        {
            "Completed" => ComicStatus.Completed,
            "Ongoing" => ComicStatus.Ongoing,
            "Hiatus" => ComicStatus.Hiatus,
            "Cancelled" => ComicStatus.Cancelled,
            _ => ComicStatus.Unknown,
        };

    private static string ToQueryString(Dictionary<string, string[]> source) =>
        string.Join(
            "&",
            source.SelectMany(kvp =>
                kvp.Value.Where(v => !string.IsNullOrEmpty(v))
                    .Select(v => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(v)}")
            )
        );
}
