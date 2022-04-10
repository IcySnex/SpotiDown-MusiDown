using SpotiDown_MusiDown.Models;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SpotiDown_MusiDown.Helpers;

public class FFmpegDownloader
{
    private static async Task<FFmpegDownload> Info() => 
        JsonSerializer.Deserialize<FFmpegDownload>(await Local.Client.GetStringAsync("https://ffbinaries.com/api/v1/version/latest"))!;

    public static OsType GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    return OsType.windows_64;
                case Architecture.X86:
                    return OsType.windows_32;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OsType.osx_64;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    return OsType.linux_64;
                case Architecture.X86:
                    return OsType.linux_32;
                case Architecture.Arm:
                    return OsType.linux_armhf;
                case Architecture.Arm64:
                    return OsType.linux_arm64;
            }
        }
        throw new Exception("Unsupported Server.", new("Server is running on an OS wich is not supported"));
    }

    public static async Task<string> GetUrl(OsType OperatingSystem)
    {
        switch (OperatingSystem)
        {
            case OsType.windows_32:
                return (await Info()).bin.windows_32.ffmpeg!;
            case OsType.windows_64:
                return (await Info()).bin.windows_64.ffmpeg!;
            case OsType.linux_32:
                return (await Info()).bin.linux_32.ffmpeg!;
            case OsType.linux_64:
                return (await Info()).bin.linux_64.ffmpeg!;
            case OsType.linux_armhf:
                return (await Info()).bin.linux_armhf.ffmpeg!;
            case OsType.linux_arm64:
                return (await Info()).bin.linux_arm64.ffmpeg!;
            case OsType.osx_64:
                return (await Info()).bin.osx_64.ffmpeg!;
            default: 
                throw new Exception("Unsupported Server.", new("Server is running on an OS wich is not supported"));
        }
    }

    public static async Task DownloadLatest()
    {
        using (FileStream fs = new(Local.GetPath("FFMPEG"), FileMode.CreateNew))
        using (ZipArchive ar = new(new MemoryStream(await Local.Client.GetByteArrayAsync(await GetUrl(GetOs())))))
            await ar.Entries.Where(z => z.Name.ToLower().Contains("ffmpeg")).First().Open().CopyToAsync(fs);
    }
}

public enum OsType
{
    windows_32,
    windows_64,
    linux_32,
    linux_64,
    linux_armhf,
    linux_arm64,
    osx_64
}