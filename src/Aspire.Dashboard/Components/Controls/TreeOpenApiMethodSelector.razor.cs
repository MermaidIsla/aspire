// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class TreeOpenApiMethodSelector : ComponentBase
{
    [Parameter, EditorRequired]
    public required Func<Task> HandleSelectedTreeItemChangedAsync { get; set; }

    [Parameter, EditorRequired]
    public required OpenApi.OpenApiViewModel PageViewModel { get; set; }

    [Parameter]
    public bool IncludeLabel { get; set; }
}
