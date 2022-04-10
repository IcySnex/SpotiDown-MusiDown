using Microsoft.AspNetCore.Mvc;
using SpotiDown_MusiDown.Models;
using SpotiDown_MusiDown.Helpers;

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
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string q, int page)
    {
        if (string.IsNullOrWhiteSpace(q) || page < 0)
            return Error.BadRequest;

        var result = await Youtube.Search(q, (page + 1) * 10, cancellationToken);
        if (result is null || result.Length < 1)
            return Error.NoContent;

        return Ok(new SearchResponse(result.Skip(Math.Max(0, result.Length - 10)).ToList(), true, ""));
    }

    [Route("youtube/api/download")]
    [HttpGet]
    public async Task<IActionResult> Download(CancellationToken cancellationToken, string id, int quality = 160)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Error.BadRequest;
        if (quality < 32 || quality > 360)
            quality = 128;

        Stream stream = await Youtube.GetStream(id, quality, cancellationToken);
        if (stream is null || stream.Length < 1)
            return Error.NoContent;

        return File((await Local.RunFFmpeg(stream, $"-i - -f mp3 -b:a {quality}k -", cancellationToken)), "application/octet-stream", $"{id}.mp3");
    }

    [Route("youtube/api/preview")]
    [HttpGet]
    public async Task<IActionResult> Preview(CancellationToken cancellationToken, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Error.BadRequest;

        Stream stream = await Youtube.GetStream(id, 64, cancellationToken);
        if (stream is null || stream.Length < 1)
            return Error.NoContent;

        var dur = await Youtube.GetDuration(id);
        int start = 50;
        int max = 20;
        while (start >= dur.TotalSeconds- max && start > 0)
            start -= 1;
        while (max > dur.TotalSeconds - start)
            max -= 1;

        return File((await Local.RunFFmpeg(stream, $"-ss {start} -t {max} -i - -f mp3 -b:a 64k -", cancellationToken)), "application/octet-stream", $"Preview-{id}.mp3");
    }

}