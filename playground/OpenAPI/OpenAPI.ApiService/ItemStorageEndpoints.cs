// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace OpenAPI.ApiService;

internal static class ItemStorageEndpoints
{
    private static readonly Dictionary<Guid, Item> s_items = new Dictionary<Guid, Item>();

    public static void MapItemStorageEndpoints(this WebApplication app)
    {
        app.MapDelete("/items/{id}", (Guid id) =>
        {
            if (s_items.Remove(id))
            {
                return Results.Content("Item deleted!", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status200OK);
            }

            return Results.Content("Item not found!", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status404NotFound);
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.TextPlain)
            .Produces<string>(StatusCodes.Status404NotFound, ContentTypes.TextPlain)
            .WithSummary("Deletes an item.")
            .WithTags("ItemStorage");

        app.MapGet("/items/", () =>
        {
            if (s_items.Count == 0)
            {
                return Results.NoContent();
            }

            return Results.Ok(s_items);
        })
            .Produces<Dictionary<Guid, Item>>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
            .Produces(StatusCodes.Status204NoContent)
            .WithSummary("Gets all items.")
            .WithTags("ItemStorage");

        app.MapGet("/items/{id}", (Guid id) =>
        {
            if (s_items.TryGetValue(id, out var item))
            {
                return Results.Ok(item);
            }

            return Results.Content("Item not found!", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status404NotFound);
        })
            .Produces<Item>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
            .Produces<string>(StatusCodes.Status404NotFound, ContentTypes.TextPlain)
            .WithSummary("Gets an item.")
            .WithTags("ItemStorage");

        app.MapPost("/items/{name}", (string name, int amount = 1, string description = "") =>
        {
            var item = new Item
            {
                Amount = amount,
                Description = description,
                Id = Guid.NewGuid(),
                Name = name
            };

            s_items[item.Id] = item;

            return Results.Created($"/items/{item.Id}", item);
        })
            .Produces<Item>(StatusCodes.Status201Created, ContentTypes.ApplicationJson)
            .WithSummary("Creates an item.")
            .WithTags("ItemStorage");

        app.MapPut("/items/{id}", (Guid id, int? amount, string? description, string? name) =>
        {
            if (!s_items.TryGetValue(id, out var item))
            {
                return Results.Content("Item not found!", ContentTypes.TextPlain, Encoding.UTF8, StatusCodes.Status404NotFound);
            }

            if (amount is not null)
            {
                item.Amount = (int)amount;
            }

            if (description is not null)
            {
                item.Description = description;
            }

            if (name is not null)
            {
                item.Name = name;
            }

            return Results.Ok(item);
        })
            .Produces<Item>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
            .Produces<string>(StatusCodes.Status404NotFound, ContentTypes.TextPlain)
            .WithSummary("Updates an item.")
            .WithTags("ItemStorage");
    }
}

internal sealed class Item
{
    public required int Amount { get; set; }
    public required string Description { get; set; }
    public required Guid Id { get; init; }
    public required string Name { get; set; }
}
