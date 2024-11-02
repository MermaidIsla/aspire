// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class TreeOpenApiMethodSelector
{
    [Parameter, EditorRequired]
    public required Func<Task> HandleSelectedTreeItemChangedAsync { get; init; }

    [Parameter, EditorRequired]
    public required Dictionary<string, List<OpenApiMethod>> Tags { get; init; }

    [Parameter]
    public bool IncludeLabel { get; set; }

    [Parameter]
    public FluentTreeItem? SelectedTreeItem { get; set; }
}
