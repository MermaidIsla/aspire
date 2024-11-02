// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Components.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Dialogs;

public sealed partial class PropertyGridEditorDialog<TItem> where TItem : IPropertyGridExtendedItem
{
    [Parameter, EditorRequired]
    public required PropertyGridEditorDialogViewModel Content { get; set; }

    private async Task OnValueChanged(TItem item)
    {
        Content.Properties = Content.Properties.Select(x => x.Name == item.Name ? item : x);

        await Content.HandlerPropertiesChanged(Content.Properties);
    }

    public class PropertyGridEditorDialogViewModel
    {
        public required string? EditingText { get; init; }
        public required Func<IQueryable<TItem>, Task> HandlerPropertiesChanged { get; init; }
        public required IQueryable<TItem> Properties { get; set; }
    }

    public static async Task OpenDialogAsync(ViewportInformation viewportInformation, IDialogService dialogService, string? editingText, IQueryable<TItem> properties, Func<IQueryable<TItem>, Task> handlerPropertiesChanged)
    {
        var width = viewportInformation.IsDesktop ? "75vw" : "100vw";
        var parameters = new DialogParameters
        {
            Width = $"min(1000px, {width})",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true,
        };

        await dialogService.ShowDialogAsync<PropertyGridEditorDialog<TItem>>(new PropertyGridEditorDialogViewModel
        {
            EditingText = editingText,
            HandlerPropertiesChanged = handlerPropertiesChanged,
            Properties = properties
        }, parameters);
    }
}
