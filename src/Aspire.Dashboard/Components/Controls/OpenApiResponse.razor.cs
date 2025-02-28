// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class OpenApiResponse : ComponentBase
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Parameter, EditorRequired]
    public required KeyValuePair<string, HttpResponseMessage> Response { get; set; }

    private string _body = string.Empty;
    private string _filter = string.Empty;
    private IQueryable<OpenApiResponseHeader> _headers = null!;

    private IQueryable<OpenApiResponseHeader> FilteredHeaders => _headers.Where(header => header.MatchesFilter(_filter));

    protected override async Task OnInitializedAsync()
    {
        await UpdateResponse();
    }

    private Task OnButtonViewTraceDetailClick()
    {
        NavigationManager.NavigateTo(DashboardUrls.TraceDetailUrl(Response.Key));
        return Task.CompletedTask;
    }

    public async Task UpdateResponse()
    {
        _body = await Response.Value.Content.ReadAsStringAsync();
        _headers = Response.Value.Content.Headers.AsQueryable().Select(x => new OpenApiResponseHeader
        {
            Name = x.Key,
            Value = string.Join(',', x.Value)
        });

        await InvokeAsync(StateHasChanged);
    }

    public class OpenApiResponseHeader : IPropertyGridItem
    {
        public required string Name { get; init; }
        public required string Value { get; init; }

        public bool MatchesFilter(string filter)
        {
            return Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                   Value.Contains(filter, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
