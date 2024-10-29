// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class OpenApiResponse : ComponentBase
{
    [Parameter, EditorRequired]
    public required HttpResponseMessage Response { get; set; }

    private string _body = string.Empty;
    private string _filter = string.Empty;
    private IQueryable<OpenApiResponseHeader> _headers = null!;

    protected override async Task OnInitializedAsync()
    {
        _body = await Response.Content.ReadAsStringAsync();
        _headers = Response.Content.Headers.AsQueryable().Select(x => new OpenApiResponseHeader
        {
            Name = x.Key,
            Value = x.Value.FirstOrDefault()
        });
    }

    public class OpenApiResponseHeader : IPropertyGridItem
    {
        public required string Name { get; init; }
        public required string? Value { get; init; }
    }
}
