using Microsoft.AspNetCore.Mvc.Filters;
using SpotiDown_MusiDown.Models;
using System.Net;
using System.Text.Json;

namespace SpotiDown_MusiDown.Helpers;

public class ErrorHandler
{
    private readonly RequestDelegate _next;
    public ErrorHandler(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        try
        {
            await _next(context);
            switch (context.Response.StatusCode)
            {
                case 404:
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new Error(405, "Not found.", "API endpoint not found. Verify your request.")));
                    break;
                case 405:
                    context.Response.StatusCode = 405;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new Error(405, "Method not allowed.", "Invalid HTTP-Method used.")));
                    break;
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new Error(405, "Internal Server Error.", $"Unexspected Error ({ex.Message}).")));
        }
    }
}