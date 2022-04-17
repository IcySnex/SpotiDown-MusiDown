using SpotiDown_MusiDown.Models;
using SpotifyAPI.Web;
using System.Text.RegularExpressions;
using YoutubeExplode.Common;

namespace SpotiDown_MusiDown.Helpers;

public class Spotify
{
    private static SpotifyClient Client = new(new OAuthClient().RequestToken(new ClientCredentialsRequest("bd6035158be648b695451636f2e311f2", "0dd522b08dc646559e5fc7ef092fa9d7")).GetAwaiter().GetResult().AccessToken);

    public static int GetSearchType(string Query)
    {
        if (Query.Contains("track", StringComparison.InvariantCultureIgnoreCase) && IsValidUrl(Query))
            return 0;
        if (Query.Contains("playlist", StringComparison.InvariantCultureIgnoreCase) && IsValidUrl(Query))
            return 1;
        if (Query.Contains("album", StringComparison.InvariantCultureIgnoreCase) && IsValidUrl(Query))
            return 2;
        return 3;
    }

    public static bool IsValidUrl(string Url) =>
        Regex.IsMatch(Url, @"http(s)?\:\/\/open\.spotify\.com\/(track|playlist|album)\/.+$");

    public static string GetIdByUrl(string Url) =>
        Url.Split('?')[0].Split('/').Last();

    public static async Task<string> GetYoutubeIdAsync(string Query, CancellationToken CancellationToken = default) =>
        (await Youtube.Client.Search.GetVideosAsync(Query, CancellationToken).CollectAsync(1)).First().Id;

    public static int GetYear(string Input)
    {
        if (Input.Length == 4)
            return int.Parse(Input);
        return DateTime.Parse(Input).Year;
    }

    public static async Task<Song[]> SearchAsync(string Query, CancellationToken CancellationToken = default)
    {
        switch (GetSearchType(Query))
        {
            case 0:
                var Track = await Client.Tracks.Get(GetIdByUrl(Query));
                var Artist = string.Join(", ", Track.Artists.Select(Artist => Artist.Name));
                string Artwork = Track.Album.Images.Count > 0 ? Track.Album.Images[0].Url : "";
                return new[] { new Song(await GetYoutubeIdAsync($"{Track.Name} - {Artist}", CancellationToken), Track.Name, Artist, GetYear(Track.Album.ReleaseDate), Artwork, Artwork, 0, false, true, true) };
            case 1:
                var Playlist = await Client.Playlists.Get(GetIdByUrl(Query));
                if (Playlist.Tracks is null || Playlist.Tracks.Total < 1)
                    return Array.Empty<Song>();
                return await Task.WhenAll((await Client.PaginateAll(Playlist.Tracks)).Where(Track => Track.Track.Type == ItemType.Track).Select(async Item =>
                {
                    var Track = (FullTrack)Item.Track;
                    var Artist = string.Join(", ", Track.Artists.Select(Artist => Artist.Name));
                    string Artwork = Track.Album.Images.Count > 0 ? Track.Album.Images[0].Url : "";
                    return new Song(await GetYoutubeIdAsync($"{Track.Name} - {Artist}", CancellationToken), Track.Name, Artist, GetYear(Track.Album.ReleaseDate), Artwork, Artwork, 0, false, true, true);
                }));
            case 2:
                var Album = await Client.Albums.Get(GetIdByUrl(Query));
                string Artwork_ = Album.Images.Count > 0 ? Album.Images[0].Url : "";
                return await Task.WhenAll((await Client.PaginateAll(Album.Tracks)).Select(async Track =>
                {
                    var Artist = string.Join(", ", Track.Artists.Select(Artist => Artist.Name));
                    return new Song(await GetYoutubeIdAsync($"{Track.Name} - {Artist}", CancellationToken), Track.Name, Artist, GetYear(Album.ReleaseDate), Artwork_, Artwork_, 0, false, true, true);
                }));
            default:
                var SearchResult = (await Client.Search.Item(new(SearchRequest.Types.Track, Query))).Tracks.Items;
                if (SearchResult is null || SearchResult.Count < 1)
                    return Array.Empty<Song>();
                return await Task.WhenAll(SearchResult.Select(async Track =>
                {
                    string Artwork = Track.Album.Images.Count > 0 ? Track.Album.Images[0].Url : "";
                    var Artist = string.Join(", ", Track.Artists.Select(Artist => Artist.Name));
                    return new Song(await GetYoutubeIdAsync($"{Track.Name} - {Artist}", CancellationToken), Track.Name, Artist, GetYear(Track.Album.ReleaseDate), Artwork, Artwork, 0, false, true, true);
                }));
        }
    }
}