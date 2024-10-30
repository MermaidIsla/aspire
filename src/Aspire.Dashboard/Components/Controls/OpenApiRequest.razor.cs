// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.OpenApi.Models;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class OpenApiRequest : ComponentBase
{
    [Parameter, EditorRequired]
    public required OpenApiDocument Document { get; set; }

    [Parameter, EditorRequired]
    public required HttpMethod Method { get; set; }

    [Parameter, EditorRequired]
    public required string Path { get; set; }

    private string _body = string.Empty;
    private string _filter = string.Empty;
    private IQueryable<OpenApiRequestHeader> _headers = null!;
    private string _methodColor = null!;
    private string _methodName = null!;
    private IQueryable<OpenApiRequestParameter> _parameters = null!;
    private OpenApiServer _selectedServer = null!;
    private string _url = string.Empty;

    private IQueryable<OpenApiRequestHeader> FilteredHeaders => _headers.Where(header => header.MatchesFilter(_filter));
    private IQueryable<OpenApiRequestParameter> FilteredParameters => _parameters.Where(header => header.MatchesFilter(_filter));

    private async Task HandleSelectedServerChangedAsync()
    {
        UpdateUrl();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        await UpdateRequest();
    }

    public async Task UpdateRequest()
    {
        var selectedPath = Document.Paths.First((path) => path.Key == Path).Value;
        var selectedOperation = selectedPath.Operations.First((operation) => operation.Key.ToString().Equals(Method.ToString(), StringComparison.OrdinalIgnoreCase)).Value;

        _body = string.Empty;
        _headers = selectedOperation.Parameters
            .Where((parameter) => parameter.In == ParameterLocation.Header)
            .Select((parameter) => new OpenApiRequestHeader
            {
                Name = parameter.Name,
                Value = string.Empty
            }).AsQueryable();
        _methodColor = OpenApiUtils.GetBadgeColorFromHttpMethod(Method);
        _methodName = Method.ToString();
        _parameters = selectedOperation.Parameters
            .Where((parameter) => parameter.In == ParameterLocation.Path || parameter.In == ParameterLocation.Query)
            .Select((parameter) => new OpenApiRequestParameter
            {
                Name = parameter.Name,
                Value = string.Empty
            }).AsQueryable();
        _selectedServer = Document.Servers.First();
        UpdateUrl();

        await InvokeAsync(StateHasChanged);
    }

    private void UpdateUrl()
    {
        _url = _selectedServer.Url + Path;
    }

    public class OpenApiRequestHeader : IPropertyGridItem
    {
        public required string Name { get; init; }
        public required string Value { get; init; }

        public bool MatchesFilter(string filter)
        {
            return Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                   Value.Contains(filter, StringComparison.CurrentCultureIgnoreCase);
        }
    }

    public class OpenApiRequestParameter : IPropertyGridItem
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
