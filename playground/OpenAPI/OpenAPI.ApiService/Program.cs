using OpenAPI.ApiService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// I wasn't able to get the project running with .NET 9
// builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// I wasn't able to get the project running with .NET 9
// app.MapOpenApi();

app.MapFormatEndpoints();
app.MapItemStorageEndpoints();
app.MapStatusCodeEndpoints();

// This is just simulating an OpenApi endpoint
app.MapGet("/openapi/v1.json", async (HttpContext context) =>
{
    context.Response.ContentType = "application/json;charset=utf-8";
    await context.Response.WriteAsync("{\"openapi\":\"3.0.1\",\"info\":{\"title\":\"OpenAPI.ApiService | v1\",\"version\":\"1.0.0\"},\"servers\":[{\"url\":\"http://localhost:5225\"}],\"paths\":{\"/format/json\":{\"get\":{\"tags\":[\"Formats\"],\"summary\":\"Returns an object in a JSON format.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/format/plain-text\":{\"get\":{\"tags\":[\"Formats\"],\"summary\":\"Returns a plain text result.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/format/xml\":{\"get\":{\"tags\":[\"Formats\"],\"summary\":\"Returns an object in a XML format.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/xml\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/items/{id}\":{\"delete\":{\"tags\":[\"ItemStorage\"],\"summary\":\"Deletes an item.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}},\"404\":{\"description\":\"Not Found\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}},\"get\":{\"tags\":[\"ItemStorage\"],\"summary\":\"Gets an item.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/Item\"}}}},\"404\":{\"description\":\"Not Found\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}},\"put\":{\"tags\":[\"ItemStorage\"],\"summary\":\"Updates an item.\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\",\"format\":\"uuid\"}},{\"name\":\"amount\",\"in\":\"query\",\"schema\":{\"type\":\"integer\",\"format\":\"int32\"}},{\"name\":\"description\",\"in\":\"query\",\"schema\":{\"type\":\"string\"}},{\"name\":\"name\",\"in\":\"query\",\"schema\":{\"type\":\"string\"}}],\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/Item\"}}}},\"404\":{\"description\":\"Not Found\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/items\":{\"get\":{\"tags\":[\"ItemStorage\"],\"summary\":\"Gets all items.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"additionalProperties\":{\"$ref\":\"#/components/schemas/Item\"}}}}},\"204\":{\"description\":\"No Content\"}}}},\"/items/{name}\":{\"post\":{\"tags\":[\"ItemStorage\"],\"summary\":\"Creates an item.\",\"parameters\":[{\"name\":\"name\",\"in\":\"path\",\"required\":true,\"schema\":{\"type\":\"string\"}},{\"name\":\"amount\",\"in\":\"query\",\"schema\":{\"type\":\"integer\",\"format\":\"int32\",\"default\":1}},{\"name\":\"description\",\"in\":\"query\",\"schema\":{\"type\":\"string\",\"default\":\"\"}}],\"responses\":{\"201\":{\"description\":\"Created\",\"content\":{\"application/json\":{\"schema\":{\"$ref\":\"#/components/schemas/Item\"}}}}}}},\"/status-code/200\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 200 OK.\",\"responses\":{\"200\":{\"description\":\"OK\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/201\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 201 Created.\",\"responses\":{\"201\":{\"description\":\"Created\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/202\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 202 Accepted.\",\"responses\":{\"202\":{\"description\":\"Accepted\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/204\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 204 No Content.\",\"responses\":{\"204\":{\"description\":\"No Content\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/400\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 400 Bad Request.\",\"responses\":{\"400\":{\"description\":\"Bad Request\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/401\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 401 Unathorized.\",\"responses\":{\"401\":{\"description\":\"Unauthorized\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/404\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 404 Not Found.\",\"responses\":{\"404\":{\"description\":\"Not Found\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/405\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 405 Method Not Allowed.\",\"responses\":{\"405\":{\"description\":\"Method Not Allowed\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/500\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 500 Internal Server Error.\",\"responses\":{\"500\":{\"description\":\"Internal Server Error\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/501\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 501 Not Implemented.\",\"responses\":{\"501\":{\"description\":\"Not Implemented\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}},\"/status-code/503\":{\"get\":{\"tags\":[\"Status Codes\"],\"summary\":\"Returns a response with status code 503 Service Unavailable.\",\"responses\":{\"503\":{\"description\":\"Service Unavailable\",\"content\":{\"text/plain\":{\"schema\":{\"type\":\"string\"}}}}}}}},\"components\":{\"schemas\":{\"Item\":{\"required\":[\"amount\",\"description\",\"id\",\"name\"],\"type\":\"object\",\"properties\":{\"amount\":{\"type\":\"integer\",\"format\":\"int32\"},\"description\":{\"type\":\"string\"},\"id\":{\"type\":\"string\",\"format\":\"uuid\"},\"name\":{\"type\":\"string\"}}}}},\"tags\":[{\"name\":\"Formats\"},{\"name\":\"ItemStorage\"},{\"name\":\"Status Codes\"}]}");
})
.ExcludeFromDescription();

app.Run();
