using SpotiDown_MusiDown.Models;
using System.Diagnostics;
using System.Reflection;

namespace SpotiDown_MusiDown.Helpers;

public static class Local
{
    public static BotPackage YoutubePackage { get; set; } = new(
        "com.icysnex.spotidown.youtube",
        "SpotiDown-Youtube",
        "0.1",
        "Bot used to directly download Youtube songs.",
        ".../youtube/api",
        "search",
        "download",
        "preview",
        "", "", "");

    public static BotPackage SpotifyPackage { get; set; } = new(
        "com.icysnex.spotidown.spotify",
        "SpotiDown-Spotify",
        "0.1",
        "Bot used to download Spotify songs via Youtube.",
        ".../youtube/api",
        "search",
        "download",
        "preview",
        "", "", "");

    public static HttpClient Client = new();

    public static string GetPath(string Relative) =>
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, Relative);

    public async static Task<byte[]> StreamToBytes(Stream Stream)
    {
        using (MemoryStream ms = new()) 
        {
            await Stream.CopyToAsync(ms);
            return ms.ToArray(); 
        }
    }

    public async static Task<byte[]> RunFFmpeg(Stream Stream, string Arguments, CancellationToken cancellationToken)
    {
        if (!File.Exists(GetPath("FFMPEG")))
            await FFmpegDownloader.DownloadLatest();

        MemoryStream Result = new();
        var FFMPEG = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetPath("FFMPEG"),
                Arguments = Arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        FFMPEG.Start();

        var inputTask = Task.Run(() =>
        {
            try
            {
            using (var Input = FFMPEG.StandardInput.BaseStream)
                Stream.CopyTo(Input);
            } catch { }
        });
        var outputTask = Task.Run(() =>
        {
            FFMPEG.StandardOutput.BaseStream.CopyTo(Result);
            Result.Seek(0, SeekOrigin.Begin);
        });

        await Task.WhenAll(inputTask, outputTask);
        await FFMPEG.WaitForExitAsync(cancellationToken);

        return await StreamToBytes(Result);
    }
}