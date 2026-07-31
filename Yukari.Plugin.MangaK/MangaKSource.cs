using System.Net;
using System.Net.Http.Json;
using Yukari.Core.Models;
using Yukari.Core.Sources;
using Yukari.Plugin.MangaK.Data;

namespace Yukari.Plugin.MangaK;

[ComicSourceMetadata(
    "MangaK",
    "1.0.0+core2.4.0",
    "https://github.com/Yukari-App/Plugin.MangaK/releases",
    "https://mangak.io/static/sites/mangak/icons/favicon-32x32.png",
    "A free manga reading platform with thousands of manga, manhwa, and manhua chapters."
)]
public class MangaKSource : IComicSource, IRequiresHttpClient
{
    private const int DefaultPageSize = 24;
    private const string SourceLanguage = "en";

    private static IReadOnlyList<Filter>? _filters;
    private static IReadOnlyDictionary<string, string>? _languages;

    public IReadOnlyList<Filter> Filters =>
        _filters ??= [
            new Filter(
                Key: "content_rating",
                DisplayName: "Content Rating",
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

    public IReadOnlyDictionary<string, string> Languages =>
        _languages ??= new Dictionary<string, string> { { "en", "English" } };

    private const string BaseUrl = "https://api.mangak.io";

    private ISharedHttpClient? _httpClient;

    public void SetHttpClient(ISharedHttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<Comic>> SearchAsync(
        string query,
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var queryParams = new Dictionary<string, string[]>
        {
            ["page"] = [page.ToString()],
            ["limit"] = [DefaultPageSize.ToString()],
        };

        if (!string.IsNullOrWhiteSpace(query))
            queryParams["q"] = [query];

        if (filters.TryGetValue("genres", out var genres))
            queryParams["genres"] = [string.Join(",", genres)];

        foreach (var kvp in filters)
        {
            if (kvp.Key == "genres")
                continue;
            queryParams[kvp.Key] = kvp.Value.ToArray();
        }

        string searchUrl = $"{BaseUrl}/titles/search?{ToQueryString(queryParams)}";

        var searchResults = (await GetFromApiAsync<SearchResponse>(searchUrl, ct))?.Data.Comics;

        if (searchResults is not { Length: > 0 })
            return Array.Empty<Comic>();

        var comics = searchResults
            .Select(result =>
            {
                return new Comic(
                    Id: result.Id,
                    ComicUrl: null,
                    Slug: null,
                    Title: result.Title,
                    Author: null,
                    Description: null,
                    Tags: [],
                    Year: null,
                    CoverImageUrl: result.CoverUrl,
                    Langs: []
                );
            })
            .ToList();

        return comics;
    }

    public async Task<IReadOnlyList<Comic>> GetTrendingAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var trendingFilters = new Dictionary<string, IReadOnlyList<string>>(filters)
        {
            ["sort"] = ["views_7days"],
        };

        return await SearchAsync(string.Empty, trendingFilters, page, ct);
    }

    public async Task<Comic?> GetDetailsAsync(string comicId, CancellationToken ct = default)
    {
        string detailsUrl = $"{BaseUrl}/titles/{comicId}";

        var detailsResult = (await GetFromApiAsync<DetailsResponse>(detailsUrl, ct))?.Data.Comic;

        if (detailsResult is null)
            return null;

        return new Comic(
            Id: detailsResult.Id,
            ComicUrl: $"https://mangak.io/{detailsResult.Slug}",
            Slug: null,
            Title: detailsResult.Title,
            Author: detailsResult.Authors.FirstOrDefault()?.Name,
            Description: detailsResult.Description,
            Tags: detailsResult.Genres.Select(t => t.Name).ToArray(),
            Year: int.TryParse(detailsResult.Year, out int y) ? y : null,
            CoverImageUrl: detailsResult.CoverUrl,
            Langs: [SourceLanguage],
            Status: GetComicStatus(detailsResult.Status)
        );
    }

    public async Task<IReadOnlyList<Chapter>> GetAllChaptersAsync(
        string comicId,
        string language,
        CancellationToken ct = default
    )
    {
        string chaptersUrl = $"{BaseUrl}/titles/{comicId}/chapters";

        var chaptersResult = (await GetFromApiAsync<ChaptersResponse>(chaptersUrl, ct))
            ?.Data
            .Chapters;

        if (chaptersResult is not { Length: > 0 })
            return Array.Empty<Chapter>();

        // MangaK returns chapters in descending order (newest first).
        // Reverse to get chronological order (oldest first).
        var chapters = chaptersResult
            .Reverse()
            .Select(c => new Chapter(
                Id: c.Id,
                Title: c.Title,
                Number: null,
                Volume: null,
                Language: language,
                Groups: Array.Empty<string>(),
                LastUpdate: DateOnly.FromDateTime(c.UpdatedAt.DateTime),
                Pages: null
            ))
            .ToList();

        return chapters;
    }

    public async Task<IReadOnlyList<ChapterPage>> GetChapterPagesAsync(
        string comicId,
        string chapterId,
        CancellationToken ct = default
    )
    {
        string pagesUrl = $"{BaseUrl}/titles/{comicId}/chapters/{chapterId}";

        var chapterResult = (await GetFromApiAsync<ChapterDetailsResponse>(pagesUrl, ct))
            ?.Data
            .Chapter;

        if (chapterResult is not { Pages.Length: > 0 })
            return Array.Empty<ChapterPage>();

        var pages = new List<ChapterPage>(chapterResult.Pages.Length);
        for (int i = 0; i < chapterResult.Pages.Length; i++)
        {
            pages.Add(new ChapterPage(Number: i + 1, ImageUrl: chapterResult.Pages[i]));
        }

        return pages;
    }

    public async Task<byte[]?> GetImageBytesAsync(string imageUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        request.Headers.Add("Referer", "https://mangak.io");

        using var response = await _httpClient!.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new HttpRequestException(
                "MangaK Rate Limit Exceeded. Try again later.",
                null,
                HttpStatusCode.TooManyRequests
            );

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

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
