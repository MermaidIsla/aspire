using OpenAPI.ApiService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();

app.MapFormatEndpoints();
app.MapItemStorageEndpoints();
app.MapStatusCodeEndpoints();

app.Run();
