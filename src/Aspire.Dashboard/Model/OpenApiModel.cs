// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Aspire.Dashboard.Model;

public sealed class OpenApiModel
{
    public required OpenApiDiagnostic Diagnostic { get; init; }
    public required OpenApiDocument Document { get; init; }
    public required Dictionary<string, List<OpenApiMethod>> Tags { get; init; }

    public OpenApiOperation GetOperationFromMethod(OpenApiMethod method)
    {
        var result = Document.Paths.First((path) => path.Key == method.Path);
        return result.Value.Operations.First((operation) => operation.Key.ToString().Equals(method.MethodName, StringComparison.OrdinalIgnoreCase)).Value;
    }
}
