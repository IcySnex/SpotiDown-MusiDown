using Microsoft.AspNetCore.Mvc;

namespace SpotiDown_MusiDown.Models;

public class Error
{
    public Error(int status, string title, string? detail = null)
    {
        this.status = status;
        this.title = title;
        this.detail = detail;
    }

    public int status { get; set; }
    public string title { get; set; }
    public string? detail { get; set; }

    public static ObjectResult New(int Code, string title, string? detail = null) =>
        new ObjectResult(new Error(Code, title, detail)) { StatusCode = Code };
    public static ObjectResult NoContent =
        new ObjectResult(new Error(204, "No Content.", "Search returned no songs.")) { StatusCode = 204 };
    public static ObjectResult BadRequest =
        new ObjectResult(new Error(400, "Bad Request.", "Please verify your request.")) { StatusCode = 400 };
}