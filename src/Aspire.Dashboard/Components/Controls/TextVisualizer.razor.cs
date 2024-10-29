// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Xml.Linq;
using System.Xml;
using Aspire.Dashboard.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace Aspire.Dashboard.Components.Controls;

public sealed partial class TextVisualizer : ComponentBase, IAsyncDisposable
{
    [Inject]
    public required IJSRuntime JS { get; init; }

    [Inject]
    public required ThemeManager ThemeManager { get; init; }

    [Parameter, EditorRequired]
    public required string Text { get; set; }

    private readonly string _copyButtonId = $"copy-{Guid.NewGuid():N}";
    private readonly string _logContainerId = $"log-container-{Guid.NewGuid():N}";

    // xml and json are language names supported by highlight.js
    private const string JsonFormat = "json";
    private const string PlaintextFormat = "plaintext";
    private const string XmlFormat = "xml";

    private string _formattedText = string.Empty;
    private IJSObjectReference? _jsModule;

    public string FormatKind { get; private set; } = PlaintextFormat;
    public ICollection<StringLogLine> FormattedLines { get; set; } = [];
    public string FormattedText
    {
        get => _formattedText;
        private set
        {
            _formattedText = value;
            FormattedLines = GetLines();
        }
    }

    private void ChangeFormattedText(string newFormatKind, string newText)
    {
        FormatKind = newFormatKind;
        FormattedText = newText;
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disconnectObserver");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Per https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-7.0#javascript-interop-calls-without-a-circuit
                // this is one of the calls that will fail if the circuit is disconnected, and we just need to catch the exception so it doesn't pollute the logs
            }
        }
    }

    private ICollection<StringLogLine> GetLines()
    {
        var lines = FormattedText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).ToList();

        return lines.Select((line, index) => new StringLogLine(index + 1, line, FormatKind != PlaintextFormat)).ToList();
    }

    private string GetLogContentClass()
    {
        // we support light (a11y-light-min) and dark (a11y-dark-min) themes. syntax to force a theme for highlight.js
        // is "theme-{themeName}"
        return $"log-content highlight-line language-{FormatKind} theme-a11y-{ThemeManager.EffectiveTheme.ToLower()}-min";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "/Components/Dialogs/TextVisualizerDialog.razor.js");
        }

        if (_jsModule is not null)
        {
            if (FormatKind is not PlaintextFormat)
            {
                await _jsModule.InvokeVoidAsync("connectObserver", _logContainerId);
            }
            else
            {
                await _jsModule.InvokeVoidAsync("disconnectObserver", _logContainerId);
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await ThemeManager.EnsureInitializedAsync();

        if (TryFormatJson())
        {
            return;
        }

        if (TryFormatXml())
        {
            return;
        }

        ChangeFormattedText(PlaintextFormat, Text);
    }

    private bool TryFormatJson()
    {
        try
        {
            var formattedJson = FormatJson(Text);
            ChangeFormattedText(JsonFormat, formattedJson);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private bool TryFormatXml()
    {
        try
        {
            var document = XDocument.Parse(Text);
            var stringWriter = new StringWriter();
            document.Save(stringWriter);
            ChangeFormattedText(XmlFormat, stringWriter.ToString());
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    public record StringLogLine(int LineNumber, string Content, bool IsFormatted);

    private static string FormatJson(string jsonString)
    {
        var jsonData = Encoding.UTF8.GetBytes(jsonString);

        // Initialize the Utf8JsonReader
        var reader = new Utf8JsonReader(jsonData, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Allow,
            // Increase the allowed limit to 1000. This matches the allowed limit of the writer.
            // It's ok to allow recursion here because JSON is read in a flat loop. There isn't a danger
            // of recursive method calls that cause a stack overflow.
            MaxDepth = 1000
        });

        // Use a MemoryStream and Utf8JsonWriter to write the formatted JSON
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;
                case JsonTokenType.PropertyName:
                    writer.WritePropertyName(reader.GetString()!);
                    break;
                case JsonTokenType.String:
                    writer.WriteStringValue(reader.GetString());
                    break;
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out var intValue))
                    {
                        writer.WriteNumberValue(intValue);
                    }
                    else if (reader.TryGetDouble(out var doubleValue))
                    {
                        writer.WriteNumberValue(doubleValue);
                    }
                    break;
                case JsonTokenType.True:
                    writer.WriteBooleanValue(true);
                    break;
                case JsonTokenType.False:
                    writer.WriteBooleanValue(false);
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                case JsonTokenType.Comment:
                    writer.WriteCommentValue(reader.GetComment());
                    break;
            }
        }

        writer.Flush();
        var formattedJson = Encoding.UTF8.GetString(stream.ToArray());

        return formattedJson;
    }
}
