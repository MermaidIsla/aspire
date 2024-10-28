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
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
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
.WithDescription("Deletes a user.")
.WithTags("Users");

app.MapGet("/users", () =>
{
    return Results.Ok(users.Values);
})
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
.WithDescription("Updates a user.")
.WithTags("Users");

app.MapGet("/openapi/v1.json", async (HttpContext context) =>
{
    context.Response.ContentType = "application/json;charset=utf-8";
    await context.Response.WriteAsync("{\"openapi\":\"3.0.1\",\"info\":{\"title\":\"OpenAPI.ApiService | v1\",\"version\":\"1.0.0\"},\"servers\":[{\"url\":\"http://localhost:5132\"}],\"paths\":{\"/weatherforecast\":{\"get\":{\"tags\":[\"WeatherForecast\"],\"description\":\"Shows a weather forecast for the next 5 days.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/components/schemas/WeatherForecast\"}}}}}}}},\"/user/{id}\":{\"delete\":{\"tags\":[\"Users\"],\"description\":\"Deletes a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\"}}},\"get\":{\"tags\":[\"Users\"],\"description\":\"Gets a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\"}}},\"put\":{\"tags\":[\"Users\"],\"description\":\"Updates a user.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}},{\"name\":\"name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}}],\"responses\":{\"200\":{\"description\":\"OK\"}}}},\"/users\":{\"get\":{\"tags\":[\"Users\"],\"description\":\"Gets all users.\",\"responses\":{\"200\":{\"description\":\"OK\"}}}},\"/user/{name}\":{\"post\":{\"tags\":[\"Users\"],\"description\":\"Creates a user.\",\"parameters\":[{\"name\":\"name\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\"}}],\"responses\":{\"200\":{\"description\":\"OK\"}}}}},\"components\":{\"schemas\":{\"WeatherForecast\":{\"required\":[\"date\",\"temperatureC\",\"summary\"],\"type\":\"object\",\"properties\":{\"date\":{\"type\":\"string\",\"format\":\"date\"},\"temperatureC\":{\"type\":\"integer\",\"format\":\"int32\"},\"summary\":{\"type\":\"string\",\"nullable\":true},\"temperatureF\":{\"type\":\"integer\",\"format\":\"int32\"}}}}},\"tags\":[{\"name\":\"WeatherForecast\"},{\"name\":\"Users\"}]}");
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
