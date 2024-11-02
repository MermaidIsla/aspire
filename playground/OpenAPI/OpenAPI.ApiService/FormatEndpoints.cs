// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace OpenAPI.ApiService;

internal static class FormatEndpoints
{
    public static void MapFormatEndpoints(this WebApplication app)
    {
        app.MapGet("/format/json", () =>
        {
            return Results.Ok(new
            {
                Property1 = "string",
                Property2 = Random.Shared.Next(0, 100),
                Property3 = Random.Shared.Next(2) == 0
            });
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
            .WithSummary("Returns an object in a JSON format.")
            .WithTags("Formats");

        app.MapGet("/format/plain-text", () =>
        {
            return Results.Content($"A plain text result with a random number: {Random.Shared.Next(0, 100)}", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status200OK);
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.TextPlain)
            .WithSummary("Returns a plain text result.")
            .WithTags("Formats");

        app.MapGet("/format/xml", () =>
        {
            return Results.Content($"<object><property1 Value=\"string\" /><property2 Value=\"{Random.Shared.Next(0, 100)}\" /><property3 Value=\"{Random.Shared.Next(2) == 0}\" /></object>", ContentTypes.ApplicationXml, Encoding.UTF8, StatusCodes.Status200OK);
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.ApplicationXml)
            .WithSummary("Returns an object in a XML format.")
            .WithTags("Formats");
    }
}
