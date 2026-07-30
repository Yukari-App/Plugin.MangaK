using System.Net;
using HtmlAgilityPack;
using Yukari.Core.Models;
using Yukari.Core.Sources;

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
    private static IReadOnlyList<Filter>? _filters;
    private static IReadOnlyDictionary<string, string>? _languages;

    public IReadOnlyList<Filter> Filters => _filters ??= [];

    public IReadOnlyDictionary<string, string> Languages =>
        _languages ??= new Dictionary<string, string> { { "en", "English" } };

    private const string BaseUrl = "https://mangak.io";

    private ISharedHttpClient? _httpClient;

    public void SetHttpClient(ISharedHttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<Comic>> SearchAsync(
        string query,
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Comic>> GetTrendingAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<Comic?> GetDetailsAsync(string comicId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Chapter>> GetAllChaptersAsync(
        string comicId,
        string language,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<ChapterPage>> GetChapterPagesAsync(
        string comicId,
        string chapterId,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<byte[]?> GetImageBytesAsync(string imageUrl, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
