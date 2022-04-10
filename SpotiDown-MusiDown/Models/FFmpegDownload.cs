using System.Text.Json.Serialization;

namespace SpotiDown_MusiDown.Models;

internal class FFmpegDownload
{
    public FFmpegDownload_Bin bin { get; set; } = new();
    public string version { get; set; } = "1.0";
}
internal class FFmpegDownload_Bin
{
    [JsonPropertyName("windows-32")]
    public FFmpegDownload_Bin_Os windows_32 { get; set; } = new();
    [JsonPropertyName("windows-64")]
    public FFmpegDownload_Bin_Os windows_64 { get; set; } = new();
    [JsonPropertyName("linux-32")]
    public FFmpegDownload_Bin_Os linux_32 { get; set; } = new();
    [JsonPropertyName("linux-64")]
    public FFmpegDownload_Bin_Os linux_64 { get; set; } = new();
    [JsonPropertyName("linux-armhf")]
    public FFmpegDownload_Bin_Os linux_armhf { get; set; } = new();
    [JsonPropertyName("linux-arm64")]
    public FFmpegDownload_Bin_Os linux_arm64 { get; set; } = new();
    [JsonPropertyName("osx-64")]
    public FFmpegDownload_Bin_Os osx_64 { get; set; } = new();
}

internal class FFmpegDownload_Bin_Os
{
    public string ffmpeg { get; set; } = "";
    public string ffprobe { get; set; } = "";
}