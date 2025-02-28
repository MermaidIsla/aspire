// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", true);
var sqlServer = builder.AddSqlServer("sqlServer", password)
    .WithLifetime(ContainerLifetime.Persistent);
var database = sqlServer.AddDatabase("master");

builder.AddProject<Projects.OpenAPI_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
