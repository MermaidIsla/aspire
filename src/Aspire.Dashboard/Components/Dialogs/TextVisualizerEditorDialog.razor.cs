// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Dialogs;

public sealed partial class TextVisualizerEditorDialog : ComponentBase
{
    [Parameter, EditorRequired]
    public required TextVisualizerEditorDialogViewModel Content { get; set; }

    private async Task OnValueChangedAsync(string value)
    {
        Content.Body = value;
        await Content.HandlerBodyChanged(value);
    }

    public class TextVisualizerEditorDialogViewModel
    {
        public required string Body { get; set; }
        public required string? EditingText { get; init; }
        public required Func<string, Task> HandlerBodyChanged { get; init; }
    }

    public static async Task OpenDialogAsync(ViewportInformation viewportInformation, IDialogService dialogService, string body, string? editingText, Func<string, Task> handlerBodyChanged)
    {
        var width = viewportInformation.IsDesktop ? "75vw" : "100vw";
        var parameters = new DialogParameters
        {
            Width = $"min(1000px, {width})",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true,
        };

        await dialogService.ShowDialogAsync<TextVisualizerEditorDialog>(new TextVisualizerEditorDialogViewModel
        {
            Body = body,
            EditingText = editingText,
            HandlerBodyChanged = handlerBodyChanged
        }, parameters);
    }
}
