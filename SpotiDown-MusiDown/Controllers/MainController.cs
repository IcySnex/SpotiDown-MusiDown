using Microsoft.AspNetCore.Mvc;
using SpotiDown_MusiDown.Helpers;
using SpotiDown_MusiDown.Models;

namespace SpotiDown_MusiDown.Controllers;

[ApiController]
public class MainController : ControllerBase
{

    [Route("api")]
    [HttpGet]
    public BotPackage[] Info() =>
        new[] { Local.YoutubePackage, Local.SpotifyPackage };

}