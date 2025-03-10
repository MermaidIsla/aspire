// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Dashboard.Model;

public sealed class OpenApiMethod
{
    public required string BadgeColor { get; init; }
    public required string Description { get; init; }
    public required string Id { get; init; }
    public required string MethodName { get; init; }
    public required string Path { get; init; }
}
