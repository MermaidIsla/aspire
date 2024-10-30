// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;
using Microsoft.OpenApi.Models;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class OpenApiServerSelect
{
    [Parameter, EditorRequired]
    public required IEnumerable<OpenApiServer> Servers { get; set; }

    [Parameter]
    public OpenApiServer SelectedServer { get; set; } = default!;

    [Parameter]
    public EventCallback<OpenApiServer> SelectedServerChanged { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    private const int ResourceOptionPixelHeight = 32;
    private const int MaxVisibleResourceOptions = 15;
    private const int SelectPadding = 8; // 4px top + 4px bottom

    private readonly string _selectId = $"server-select-{Guid.NewGuid():N}";

    private async Task SelectedServerChangedCore()
    {
        await SelectedServerChanged.InvokeAsync(SelectedServer);
    }

    private static void ValuedChanged(string value)
    {
        // Do nothing. Required for bunit change to trigger SelectedOptionChanged.
    }

    private static string? GetPopupHeight()
    {
        return $"{(ResourceOptionPixelHeight * MaxVisibleResourceOptions) + SelectPadding}px";
    }
}
