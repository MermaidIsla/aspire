using OpenAPI.ApiService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerClient("master");

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();

app.MapFormatEndpoints();
app.MapItemStorageEndpoints();
app.MapSqlEndpoints();
app.MapStatusCodeEndpoints();

app.Run();
