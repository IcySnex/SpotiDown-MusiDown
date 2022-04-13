using SpotiDown_MusiDown.Models;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpotiDown_MusiDown.Helpers;

public static class Local
{

    public static BotPackage YoutubePackage { get; set; } = new(
        "com.icysnex.spotidown.youtube",
        "SpotiDown-Youtube",
        "0.1",
        "Bot used to directly download Youtube songs.",
        "http://spotidown-musidown.cf/youtube/api",
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
    public static Timer TempTimer = GetTimer();

    public static Timer GetTimer()
    {
        Directory.CreateDirectory(GetPath("Temp\\"));
        return new(s => {
            Directory.Delete(GetPath("Temp\\"), true);
            Directory.CreateDirectory(GetPath("Temp\\"));
        }, null, 0, 3600000);
    }

    public static string GetPath(string Relative) =>
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, Relative);

    public async static Task<byte[]> ToBytesAsync(Stream Stream)
    {
        using (MemoryStream ms = new()) 
        {
            await Stream.CopyToAsync(ms);
            return ms.ToArray(); 
        }
    }

    public async static Task<MemoryStream> RunFFmpegAsync(Stream Stream, string Arguments, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(GetPath("FFMPEG")))
            await FFmpegDownloader.LatestAsync();

        MemoryStream Result = new();
        var FFMPEG = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"./{GetPath("FFMPEG")}" : GetPath("FFMPEG"),
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

        return Result;
    }

    public async static Task<MemoryStream> GetSongAsync(string Id, Stream Stream, int Quality, bool SaveToTemp = true, CancellationToken cancellationToken = default)
    {

        if (File.Exists(GetPath($"Temp\\{Id}.mp3")))
        {
            MemoryStream Result = new();
            using (var fs = new FileStream(GetPath($"Temp\\{Id}.mp3"), FileMode.Open))
                await fs.CopyToAsync(Result);
            Result.Seek(0, SeekOrigin.Begin);
            return Result;
        }

        var Convert = await RunFFmpegAsync(Stream, $"-i - -f mp3 -b:a {Quality}k -", cancellationToken);
        if (SaveToTemp)
            using (var fs = new FileStream(GetPath($"Temp\\{Id}.mp3"), FileMode.Create))
                await Convert.CopyToAsync(fs);
        return Convert;
    }
}