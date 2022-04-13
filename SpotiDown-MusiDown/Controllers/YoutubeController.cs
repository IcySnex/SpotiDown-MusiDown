using Microsoft.AspNetCore.Mvc;
using SpotiDown_MusiDown.Models;
using SpotiDown_MusiDown.Helpers;
using System.Text.RegularExpressions;
using System.Text.Json;

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

        var result = await Youtube.SearchAsync(q, ((page + 1) * 10) + 1, cancellationToken);
        if (result is null || result.Length < 1)
            return Error.NoContent;

        return Ok(new SearchResponse(result.Skip(page * 10).Take(10), result.Length > (page + 1) * 10, (page+1).ToString()));
    }

    [Route("youtube/api/download")]
    [HttpGet]
    public async Task<IActionResult> DownloadAsync(CancellationToken cancellationToken, string id, int quality = 128, string? optional = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Error.BadRequest;
        if (quality < 32 || quality > 360)
            quality = 128;

        Stream stream = await Youtube.GetStreamAsync(id, quality, cancellationToken);
        if (stream is null || stream.Length < 1)
            return Error.NoContent;

        return File(await Local.GetSongAsync(id, stream, quality, false, cancellationToken), "application/octet-stream", $"{id}.mp3");
    }

    [Route("youtube/api/preview")]
    [HttpGet]
    public async Task PreviewAsync(CancellationToken cancellationToken, string id, int quality = 128, string? optional = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(400, "Bad Request.", "Please verify your request.")));
            return;
        }

        Stream stream = await Youtube.GetStreamAsync(id, 64, cancellationToken);
        if (stream is null || stream.Length < 1)
        {
            Response.StatusCode = 204;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(204, "No Content.", "Search returned no songs.")));
            return;
        }

        if (!Request.Headers.ContainsKey("Range"))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(JsonSerializer.Serialize(new Error(400, "Bad Request.", "Please verify your request.")));
            return;
        }
        var match = new Regex(@"bytes=([0-9]+)\-([0-9]*)").Match(Request.Headers["Range"].First());

        var PreviewSong = await Local.GetSongAsync(id, stream, quality, true, cancellationToken);

        int start;
        int end;
        if (!int.TryParse(match.Groups[1].Value, out start))
            start = 0;
        if (!int.TryParse(match.Groups[2].Value, out end))
            end = start + 30000;
        if (end > PreviewSong.Length)
            end = (int)PreviewSong.Length;
        if (start > end)
            start = end - 30000;

        Response.StatusCode = 206;
        Response.ContentLength = end - start;
        Response.ContentType = "application/octet-stream";
        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{PreviewSong.Length}");
        Response.Headers.Add("Accept-Ranges", "bytes");
        Response.Headers.Add("Content-Disposition", $"attachment; filename=Preview-{id}.mp3");

        PreviewSong.Seek(start, SeekOrigin.Begin);
        PreviewSong.SetLength(end);
        await PreviewSong.CopyToAsync(Response.Body, cancellationToken);
        return;
    }
}