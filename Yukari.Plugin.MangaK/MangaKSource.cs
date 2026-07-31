using System.Net;
using System.Net.Http.Json;
using Yukari.Core.Models;
using Yukari.Core.Sources;
using Yukari.Plugin.MangaK.Data;

namespace Yukari.Plugin.MangaK;

[ComicSourceMetadata(
    "MangaK",
    "0.1.0-Alpha+core2.4.0",
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

    public IReadOnlyList<Filter> Filters => _filters ??= [];

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

        foreach (var kvp in filters)
            queryParams[kvp.Key] = kvp.Value.ToArray();

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
                kvp.Value.Select(v => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(v)}")
            )
        );
}
