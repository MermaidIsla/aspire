// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components;

namespace Aspire.Dashboard.Components.Dialogs;

public sealed partial class OpenApiResponseDialog : ComponentBase
{
    [Parameter]
    public OpenApiDialogViewModel Content { get; set; } = default!;

    public class OpenApiDialogViewModel
    {
        public required string Body { get; set; }
        public required string ContentType { get; set; }
        public required string StatusCode { get; set; }
    }
}
