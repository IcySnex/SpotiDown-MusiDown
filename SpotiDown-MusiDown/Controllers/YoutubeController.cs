using Microsoft.AspNetCore.Mvc;
using SpotiDown_MusiDown.Models;
using SpotiDown_MusiDown.Helpers;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http.Headers;

namespace SpotiDown_MusiDown.Controllers;

[ApiController]
public class YoutubeController : ControllerBase
{
    [Route("youtube/api")]
    [HttpGet]
    public BotPackage Package() =>
        Local.YoutubePackage;

    [Route("youtube/api/search")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync(CancellationToken cancellationToken, string q, int page, string? optional = null)
    {
        if (string.IsNullOrWhiteSpace(q) || page < 0)
            return Error.BadRequest;

        var result = await Youtube.SearchAsync(q, (page + 1) * 10, cancellationToken);
        if (result is null || result.Length < 1)
            return Error.NoContent;

        return Ok(new SearchResponse(result.Skip(Math.Max(0, result.Length - 10)).ToList(), true, "page++"));
    }

    [Route("youtube/api/download")]
    [HttpGet]
    public async Task<IActionResult> DownloadAsync(CancellationToken cancellationToken, string id, int quality = 160, string? optional = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Error.BadRequest;
        if (quality < 32 || quality > 360)
            quality = 128;

        Stream stream = await Youtube.GetStreamAsync(id, quality, cancellationToken);
        if (stream is null || stream.Length < 1)
            return Error.NoContent;

        return File(await Local.RunFFmpegAsync(stream, $"-i - -f mp3 -b:a {quality}k -", cancellationToken), "application/octet-stream", $"{id}.mp3");
    }

    [Route("youtube/api/preview")]
    [HttpGet]
    public async Task PreviewAsync(CancellationToken cancellationToken, string id, string? optional = null)
    {
        // Check if id is invalid and throw bad request if so.
        if (string.IsNullOrWhiteSpace(id))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(400, "Bad Request.", "Please verify your request.")));
            return;
        }

        // Load Song stream and check if null or empty and throw no content if so.
        Stream stream = await Youtube.GetStreamAsync(id, 64, cancellationToken);
        if (stream is null || stream.Length < 1)
        {
            Response.StatusCode = 204;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(204, "No Content.", "Search returned no songs.")));
            return;
        }

        // Check headers for "Range" key and throw bad request if not found.
        if (!Request.Headers.ContainsKey("Range"))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(400, "Bad Request.", "Please verify your request.")));
            return;
        }
        // Format range into match so we can work with it.
        var reg = new Regex(@"seconds ([0-9]+)\-([0-9]*)");
        var match = reg.Match(Request.Headers["Range"].First());

        // Get video duration.
        var dur = await Youtube.GetDurationAsync(id, cancellationToken);
        // Set start and end position by range header.
        int start = int.Parse(match.Groups[1].Value);
        int end = int.Parse(match.Groups[2].Value);

        // Check if end position is greater than total duration and modify if needed.
        if (end > dur.TotalSeconds)
            end = (int)dur.TotalSeconds;
        // Check if start position is greater than end position and subtract 10 as long as it is.
        while (start >= end)
            start -= 10;
        // Check if start position is less than 0 and modify if needed.
        if (start < 0)
            start = 0;

        // Download song stream and trim it by seconds given in header.
        var PreviewSong = await Local.RunFFmpegAsync(stream, $"-ss {start} -to {end} -i - -f mp3 -b:a 64k -", cancellationToken);

        // Format response headers and return song.
        Response.StatusCode = 206;
        Response.ContentLength = PreviewSong.Length;
        Response.ContentType = "application/octet-stream";
        Response.Headers.Add("Content-Range", $"seconds {start}-{end}/*");
        Response.Headers.Add("Accept-Ranges", "seconds");
        Response.Headers.Add("Content-Disposition", $"attachment; filename=Preview-{id}.mp3");
        await PreviewSong.CopyToAsync(Response.Body, cancellationToken);
        return;
    }

}