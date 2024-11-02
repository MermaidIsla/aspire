// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Aspire.Dashboard.Components.Dialogs;
using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.OpenApi.Models;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class OpenApiRequest : ComponentBase
{
    [CascadingParameter]
    public required ViewportInformation ViewportInformation { get; init; }

    [Inject]
    public required IDialogService DialogService { get; init; }

    [Parameter, EditorRequired]
    public required OpenApiDocument Document { get; set; }

    [Parameter, EditorRequired]
    public required HttpMethod Method { get; set; }

    [Parameter, EditorRequired]
    public required string Path { get; set; }

    private readonly string _editHeadersButtonId = $"edit-headers-{Guid.NewGuid():N}";
    private readonly string _editParametersButtonId = $"edit-parameters-{Guid.NewGuid():N}";

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

    public HttpRequestMessage CreateHttpRequest()
    {
        var url = _url;

        foreach (var parameter in _parameters)
        {
            if (string.IsNullOrEmpty(parameter.Value))
            {
                if (parameter.IsRequired)
                {
                    throw new ArgumentNullException();
                }

                continue;
            }

            if (parameter.In == ParameterLocation.Path)
            {
                url = url.Replace($"{{{parameter.Name}}}", Uri.EscapeDataString(parameter.Value));
            }
            else if (parameter.In == ParameterLocation.Query)
            {
                url = QueryHelpers.AddQueryString(url, parameter.Name, parameter.Value);
            }
            else
            {
                throw new NotSupportedException();
            }
        };

        var request = new HttpRequestMessage
        {
            Method = Method,
            RequestUri = new Uri(url),
        };

        foreach (var header in _headers)
        {
            request.Headers.Add(header.Name, header.Value);
        };

        return request;
    }

    private async Task HandleSelectedServerChangedAsync()
    {
        UpdateUrl();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnButtonEditHeadersClick()
    {
        await PropertyGridEditorDialog<OpenApiRequestHeader>.OpenDialogAsync(ViewportInformation, DialogService, "Editing headers", _headers, async (result) =>
        {
            _headers = result;
            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task OnButtonEditParametersClick()
    {
        await PropertyGridEditorDialog<OpenApiRequestParameter>.OpenDialogAsync(ViewportInformation, DialogService, "Editing parameters", _parameters, async (result) =>
        {
            _parameters = result;
            await InvokeAsync(StateHasChanged);
        });
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
                IsRequired = parameter.Required,
                Name = parameter.Name,
                Value = string.Empty,
                ValueType = string.IsNullOrEmpty(parameter.Schema.Format) ? parameter.Schema.Type : parameter.Schema.Format
            }).AsQueryable();
        _methodColor = OpenApiUtils.GetBadgeColorFromHttpMethod(Method);
        _methodName = Method.ToString();
        _parameters = selectedOperation.Parameters
            .Where((parameter) => parameter.In == ParameterLocation.Path || parameter.In == ParameterLocation.Query)
            .Select((parameter) => new OpenApiRequestParameter
            {
                In = parameter.In ?? throw new NotSupportedException(),
                IsRequired = parameter.Required,
                Name = parameter.Name,
                Value = string.Empty,
                ValueType = string.IsNullOrEmpty(parameter.Schema.Format) ? parameter.Schema.Type : parameter.Schema.Format
            }).AsQueryable();
        _selectedServer = Document.Servers.First();
        UpdateUrl();

        await InvokeAsync(StateHasChanged);
    }

    private void UpdateUrl()
    {
        _url = _selectedServer.Url + Path;
    }

    public bool ValidateUserInput([MaybeNullWhen(true)] out string error)
    {
        foreach (var header in _headers)
        {
            if (header.IsRequired && string.IsNullOrEmpty(header.Value))
            {
                error = $"Header \"{header.Name}\" is required but its value is null!";
                return false;
            }
        };

        foreach (var parameter in _parameters)
        {
            if (parameter.IsRequired && string.IsNullOrEmpty(parameter.Value))
            {
                error = $"Parameter \"{parameter.Name}\" is required but its value is null!";
                return false;
            }
        };

        error = null;
        return true;
    }

    public class OpenApiRequestHeader : IPropertyGridExtendedItem
    {
        public required bool IsRequired { get; init; }
        public required string Name { get; init; }
        public required string? Value { get; set; }
        public required string ValueType { get; init; }

        public bool MatchesFilter(string filter)
            => Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
               Value?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) == true;
    }

    public class OpenApiRequestParameter : IPropertyGridExtendedItem
    {
        public required ParameterLocation In { get; init; }
        public required bool IsRequired { get; init; }
        public required string Name { get; init; }
        public required string? Value { get; set; }
        public required string ValueType { get; init; }

        public bool MatchesFilter(string filter)
            => Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
               Value?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) == true;
    }
}
