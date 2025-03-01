// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace OpenAPI.ApiService;

internal static class SqlEndpoints
{
    public static void MapSqlEndpoints(this WebApplication app)
    {
        app.MapGet("/sql/select", async (SqlConnection connection) =>
        {
            await connection.OpenAsync().ConfigureAwait(false);

            var result = new List<int>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select 1 union select 2 union select 3";

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(0));
                    }
                }
            }

            return Results.Ok(result);
        })
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.ApplicationJson)
            .WithSummary("Returns some data from a Sql Server.")
            .WithTags("Sql");
    }
}
