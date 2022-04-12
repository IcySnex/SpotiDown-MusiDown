using SpotiDown_MusiDown.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace SpotiDown_MusiDown.Helpers;

public static class Youtube
{
    private static YoutubeClient Client = new();

    public static int GetSearchType(string Query)
    {
        if (VideoId.TryParse(Query) is VideoId)
            return 0;
        if (PlaylistId.TryParse(Query) is PlaylistId)
            return 1;
        if (ChannelId.TryParse(Query) is ChannelId)
            return 2;
        return 3;
    }

    public static async Task<string> GetThumbnailAsync(string Id)
    {
        try
        {
            string Url = $"https://i3.ytimg.com/vi/{Id}/maxresdefault.jpg";
            await Local.Client.GetByteArrayAsync(Url);
            return Url;
        }
        catch { return $"https://i3.ytimg.com/vi/{Id}/hqdefault.jpg"; }
    }

    public static async Task<Song[]> SearchAsync(string Query, int ResultCount = 50, CancellationToken CancellationToken = default)
    {
        switch (GetSearchType(Query))
        {
            case 0:
                var Video = await Client.Videos.GetAsync(Query, CancellationToken);
                return new[] { new Song(Video.Id, Video.Title, Video.Author.Title, Video.UploadDate.Year, Video.Thumbnails[0].Url, await GetThumbnailAsync(Video.Id), 0, false, true, true) };
            case 1:
                return await Task.WhenAll((await Client.Playlists.GetVideosAsync(Query, CancellationToken).CollectAsync(ResultCount)).Select(async Video => new Song(Video.Id, Video.Title, Video.Author.Title, DateTime.Now.Year, Video.Thumbnails[0].Url, await GetThumbnailAsync(Video.Id), 0, false, true, true)));
            case 2:
                return await Task.WhenAll((await Client.Channels.GetUploadsAsync(Query, CancellationToken).CollectAsync(ResultCount)).Select(async Video => new Song(Video.Id, Video.Title, Video.Author.Title, DateTime.Now.Year, Video.Thumbnails[0].Url, await GetThumbnailAsync(Video.Id), 0, false, true, true)));
            default:
                return await Task.WhenAll((await Client.Search.GetVideosAsync(Query, CancellationToken).CollectAsync(ResultCount)).Select(async Video => new Song(Video.Id, Video.Title, Video.Author.Title, DateTime.Now.Year, Video.Thumbnails[0].Url, await GetThumbnailAsync(Video.Id), 0, false, true, true)));
        }
    }

    public static async Task<TimeSpan> GetDurationAsync(string Id, CancellationToken CancellationToken = default) =>
        (await Client.Videos.GetAsync(Id, CancellationToken)).Duration is TimeSpan ts ? ts : new(0);

    public static async Task<Stream> GetStreamAsync(string Input, double Quality, CancellationToken CancellationToken = default)
    {
        var Avaiable = (await Client.Videos.Streams.GetManifestAsync(Input, CancellationToken)).GetAudioOnlyStreams();
        var Info = Avaiable.First(n => Math.Abs(Quality - n.Bitrate.KiloBitsPerSecond) == Avaiable.Min(n => Math.Abs(Quality - n.Bitrate.KiloBitsPerSecond)));
        return await Client.Videos.Streams.GetAsync(Info, CancellationToken);
    }
}
