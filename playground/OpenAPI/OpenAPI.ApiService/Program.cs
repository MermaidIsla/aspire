var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// I wasn't able to get the project running with .NET 9
// builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// I wasn't able to get the project running with .NET 9
// app.MapOpenApi();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.Produces<User>(statusCode: 200, contentType: "application/json")
.WithDescription("Shows a weather forecast for the next 5 days.")
.WithTags("WeatherForecast");

var users = new Dictionary<Guid, User>();

app.MapDelete("/user/{id}", (Guid id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        users.Remove(id);
        return Results.Ok();
    }

    return Results.BadRequest("User not found!");
})
.Produces<string>(statusCode: 200, contentType: "text/plain")
.Produces<string>(statusCode: 400, contentType: "text/plain")
.WithDescription("Deletes a user.")
.WithTags("Users");

app.MapGet("/users", () =>
{
    return Results.Ok(users.Values);
})
.Produces<User>(statusCode: 200, contentType: "application/json")
.WithDescription("Gets all users.")
.WithTags("Users");

app.MapGet("/user/{id}", (Guid id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        return Results.Ok(user);
    }

    return Results.BadRequest("User not found!");
})
.Produces<User>(statusCode: 200, contentType: "application/json")
.Produces<string>(statusCode: 400, contentType: "text/plain")
.WithDescription("Gets a user.")
.WithTags("Users");

app.MapPost("/user/{name}", (string name) =>
{
    User user = new User
    {
        Id = Guid.NewGuid(),
        Name = name
    };
    users[user.Id] = user;
    return Results.Ok(user);
})
.Produces<User>(statusCode: 200, contentType: "application/json")
.WithDescription("Creates a user.")
.WithTags("Users");

app.MapPut("/user/{id}", (Guid id, string name) =>
{
    if (users.TryGetValue(id, out var user))
    {
        user.Name = name;
        return Results.Ok(user);
    }

    return Results.BadRequest("User not found!");
})
.Produces<User>(statusCode: 200, contentType: "application/json")
.Produces<string>(statusCode: 400, contentType: "text/plain")
.WithDescription("Updates a user.")
.WithTags("Users");

app.MapGet("/format/json", () =>
{
    return Results.Ok(new
    {
        Property1 = "string",
        Property2 = Random.Shared.Next(0, 100),
        Property3 = Random.Shared.Next(2) == 0
    });
})
.Produces<string>(statusCode: 200, contentType: "application/json")
.WithDescription("Returns an object in a JSON format.")
.WithTags("Formats");

app.MapGet("/format/plain-text", () =>
{
    return Results.Ok($"A plain text result with a random number: {Random.Shared.Next(0, 100)}");
})
.Produces<string>(statusCode: 200, contentType: "text/plain")
.WithDescription("Returns a plain text result.")
.WithTags("Formats");

app.MapGet("/format/xml", () =>
{
    return Results.Content($"<object><property1 Value=\"string\" /><property2 Value=\"{Random.Shared.Next(0, 100)}\" /><property3 Value=\"{Random.Shared.Next(2) == 0}\" /></object>", contentType: "application/xml", statusCode: 200);
})
.Produces<string>(statusCode: 200, contentType: "application/xml")
.WithDescription("Returns an object in a XML format.")
.WithTags("Formats");

app.MapGet("/openapi/v1.json", async (HttpContext context) =>
{
    context.Response.ContentType = "application/json;charset=utf-8";
    await context.Response.WriteAsync("{\"openapi\":\"3.0.1\",\"info\":{\"title\":\"OpenAPI.ApiService | v1\",\"version\":\"1.0.0\"},\"servers\":[{\"url\":\"http://localhost:5225\"}],\"paths\":{\"/weatherforecast\":{\"get\":{\"tags\":[\"WeatherForecast\"],\"description\":\"Shows a weather forecast for the next 5 days.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/User\"}}}}}}},\"/user/{id}\":{\"delete\":{\"tags\":[\"Users\"],\"description\":\"Deletes a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}},\"400\":{\"description\":\"Bad Request\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}},\"get\":{\"tags\":[\"Users\"],\"description\":\"Gets a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/User\"}}}},\"400\":{\"description\":\"Bad Request\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}},\"put\":{\"tags\":[\"Users\"],\"description\":\"Updates a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}},{\"name\":\"name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/User\"}}}},\"400\":{\"description\":\"Bad Request\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/users\":{\"get\":{\"tags\":[\"Users\"],\"description\":\"Gets all users.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/User\"}}}}}}},\"/user/{name}\":{\"post\":{\"tags\":[\"Users\"],\"description\":\"Creates a user.\",\"parameters\":[{\"name\":\"name\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/User\"}}}}}}},\"/format/json\":{\"get\":{\"tags\":[\"Formats\"],\"description\":\"Returns an object in a JSON format.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/format/plain-text\":{\"get\":{\"tags\":[\"Formats\"],\"description\":\"Returns a plain text result.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/format/xml\":{\"get\":{\"tags\":[\"Formats\"],\"description\":\"Returns an object in a XML format.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/xml\":{\"schema\":{\"type\":\"string\"}}}}}}}},\"components\":{\"schemas\":{\"User\":{\"required\":[\"name\"],\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"string\",\"format\":\"uuid\"},\"name\":{\"type\":\"string\"}}}}},\"tags\":[{\"name\":\"WeatherForecast\"},{\"name\":\"Users\"},{\"name\":\"Formats\"}]}");
})
.ExcludeFromDescription();

app.Run();

sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

sealed class User
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
}
