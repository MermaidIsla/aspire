// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace OpenAPI.ApiService;

public static class StatusCodeEndpoints
{
    public static void MapStatusCodeEndpoints(this WebApplication app)
    {
        // 2xx codes

        app.MapGet("/status-code/200", (ILogger<Program> logger, HttpContext context) =>
        {
            logger.LogInformation("Count: {Count}", context.Request.Headers.Count);
            foreach (var pair in context.Request.Headers)
            {
                logger.LogInformation("{Key}: {Value}", pair.Key, string.Join(',', pair.Value.AsEnumerable()));
            }

            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status200OK);
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 200 OK.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/201", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status201Created);
        })
            .Produces<string>(StatusCodes.Status201Created, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 201 Created.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/202", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status202Accepted);
        })
            .Produces<string>(StatusCodes.Status202Accepted, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 202 Accepted.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/204", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status204NoContent);
        })
            .Produces<string>(StatusCodes.Status204NoContent, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 204 No Content.")
            .WithTags("Status Codes");

        // 4xx codes

        app.MapGet("/status-code/400", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status400BadRequest);
        })
            .Produces<string>(StatusCodes.Status400BadRequest, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 400 Bad Request.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/401", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status401Unauthorized);
        })
            .Produces<string>(StatusCodes.Status401Unauthorized, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 401 Unathorized.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/404", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status404NotFound);
        })
            .Produces<string>(StatusCodes.Status404NotFound, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 404 Not Found.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/405", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status405MethodNotAllowed);
        })
            .Produces<string>(StatusCodes.Status405MethodNotAllowed, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 405 Method Not Allowed.")
            .WithTags("Status Codes");

        // 5xx codes

        app.MapGet("/status-code/500", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status500InternalServerError);
        })
            .Produces<string>(StatusCodes.Status500InternalServerError, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 500 Internal Server Error.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/501", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status501NotImplemented);
        })
            .Produces<string>(StatusCodes.Status501NotImplemented, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 501 Not Implemented.")
            .WithTags("Status Codes");

        app.MapGet("/status-code/503", () =>
        {
            return Results.Content("", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status503ServiceUnavailable);
        })
            .Produces<string>(StatusCodes.Status503ServiceUnavailable, ContentTypes.TextPlain)
            .WithSummary("Returns a response with status code 503 Service Unavailable.")
            .WithTags("Status Codes");
    }
}
