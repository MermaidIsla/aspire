// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Dialogs;

public sealed partial class TextVisualizerEditorDialog
{
    [Parameter, EditorRequired]
    public required string Body { get; set; }

    [Parameter]
    public string? EditingText { get; set; }
}
