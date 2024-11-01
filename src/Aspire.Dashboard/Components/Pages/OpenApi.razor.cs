// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Aspire.Dashboard.Components.Controls;
using Aspire.Dashboard.Components.Layout;
using Aspire.Dashboard.Extensions;
using Aspire.Dashboard.Model;
using Aspire.Dashboard.Model.Otlp;
using Aspire.Dashboard.Otlp.Model;
using Aspire.Dashboard.Resources;
using Aspire.Dashboard.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Aspire.Dashboard.Components.Pages;

public sealed partial class OpenApi : ComponentBase, IAsyncDisposable, IPageWithSessionAndUrlState<OpenApi.OpenApiViewModel, OpenApi.OpenApiPageState>
{
    [CascadingParameter]
    public required ViewportInformation ViewportInformation { get; init; }

    [Inject]
    public required IStringLocalizer<ControlsStrings> ControlsStringsLoc { get; init; }

    [Inject]
    public required IDashboardClient DashboardClient { get; init; }

    [Inject]
    public required IDialogService DialogService { get; init; }

    [Inject]
    public required HttpClient HttpClient { get; init; }

    [Inject]
    public required IStringLocalizer<Dashboard.Resources.OpenApi> Loc { get; init; }

    [Inject]
    public required ILogger<OpenApi> Logger { get; init; }

    [Inject]
    public required NavigationManager NavigationManager { get; init; }

    [Inject]
    public required ISessionStorage SessionStorage { get; init; }

    [Parameter]
    public string? ResourceName { get; set; }

    private OpenApiSubscription? _openApiSubscription;
    private OpenApiRequest? _request;
    private Controls.OpenApiResponse? _response;
    private ImmutableList<SelectViewModel<ResourceTypeDetails>>? _resources;
    private readonly ConcurrentDictionary<string, ResourceViewModel> _resourceByName = new(StringComparers.ResourceName);
    private readonly CancellationTokenSource _resourceSubscriptionCts = new();
    private Task? _resourceSubscriptionTask;
    private CancellationToken _resourceSubscriptionToken;
    private SummaryDetailsView<HttpResponseMessage>? _summaryDetailsView;
    private TreeOpenApiMethodSelector? _treeOpenApiMethodSelector;

    // UI
    private SelectViewModel<ResourceTypeDetails> _noSelection = null!;
    private AspirePageContentLayout? _contentLayout;

    // State
    public OpenApiViewModel PageViewModel { get; set; } = null!;

    public string BasePath => DashboardUrls.OpenApiBasePath;
    public string SessionStorageKey => BrowserStorageKeys.OpenApiPageState;

    private void ClearMethodResponse()
    {
        if (PageViewModel.SelectedMethod is null)
        {
            return;
        }

        PageViewModel.SelectedMethod.Response = null;
    }

    public OpenApiPageState ConvertViewModelToSerializable()
    {
        return new OpenApiPageState
        {
            SelectedResource = PageViewModel.SelectedResource?.Name
        };
    }

    public async ValueTask DisposeAsync()
    {
        _resourceSubscriptionCts.Cancel();
        _resourceSubscriptionCts.Dispose();
        await TaskHelpers.WaitIgnoreCancelAsync(_resourceSubscriptionTask);

        await StopAndClearOpenApiSubscriptionAsync();
    }

    public string GetUrlFromSerializableViewModel(OpenApiPageState serializable)
    {
        return DashboardUrls.OpenApiUrl(serializable.SelectedResource);
    }

    private async Task HandleSelectedOptionChangedAsync()
    {
        PageViewModel.SelectedResource = PageViewModel.SelectedOption?.Id?.InstanceId is null ? null : _resourceByName[PageViewModel.SelectedOption.Id.InstanceId];
        await this.AfterViewModelChangedAsync(_contentLayout, waitToApplyMobileChange: true);
    }

    private async Task HandleSelectedTreeItemChangedAsync()
    {
        if (PageViewModel.SelectedTreeItem?.Data is OpenApiViewModelMethod method)
        {
            PageViewModel.SelectedMethod = method;
        }
        else
        {
            PageViewModel.SelectedMethod = null;
        }

        await InvokeAsync(StateHasChanged);

        if (_request is not null)
        {
            await _request.UpdateRequest();
        }

        await this.AfterViewModelChangedAsync(_contentLayout, waitToApplyMobileChange: true);
    }

    private async Task LoadOpenAPISpecification()
    {
        if (PageViewModel.SelectedResource == null)
        {
            return;
        }

        foreach (var url in PageViewModel.SelectedResource.Urls)
        {
            if (url.IsInternal)
            {
                continue;
            }

            var baseUrl = url.Url.Scheme + "://" + url.Url.Host + ":" + url.Url.Port;

            var response = await HttpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseUrl + "/openapi/v1.json")
            });

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("Error while trying to get OpenAPI specification!");
                return;
            }

            var reader = new OpenApiStringReader();
            PageViewModel.Document = reader.Read(await response.Content.ReadAsStringAsync(), out _);
            ParseOpenApiDocument(baseUrl);

            await InvokeAsync(StateHasChanged);
            break;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _resourceSubscriptionToken = _resourceSubscriptionCts.Token;
        _noSelection = new SelectViewModel<ResourceTypeDetails>
        {
            Id = null,
            Name = ControlsStringsLoc[nameof(ControlsStrings.LabelNone)]
        };
        PageViewModel = new OpenApiViewModel
        {
            Document = null,
            Methods = null,
            SelectedOption = _noSelection,
            SelectedResource = null
        };

        var loadingTcs = new TaskCompletionSource();

        await TrackResourceSnapshotsAsync();

        // Wait for resource to be selected. If selected resource isn't available after a few seconds then stop waiting.
        try
        {
            await loadingTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), _resourceSubscriptionToken);
            Logger.LogDebug("Loading task completed.");
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Load task canceled.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Load timeout while waiting for resource {ResourceName}.", ResourceName);
        }

        async Task TrackResourceSnapshotsAsync()
        {
            if (!DashboardClient.IsEnabled)
            {
                return;
            }

            var (snapshot, subscription) = await DashboardClient.SubscribeResourcesAsync(_resourceSubscriptionToken);

            Logger.LogDebug("Received initial resource snapshot with {ResourceCount} resources.", snapshot.Length);

            foreach (var resource in snapshot)
            {
                var added = _resourceByName.TryAdd(resource.Name, resource);
                Debug.Assert(added, "Should not receive duplicate resources in initial snapshot data.");
            }

            UpdateResourcesList();

            // Set loading task result if the selected resource is already in the snapshot or there is no selected resource.
            if (ResourceName != null)
            {
                if (_resourceByName.TryGetValue(ResourceName, out var selectedResource))
                {
                    SetSelectedResourceOption(selectedResource);
                }
            }
            else
            {
                Logger.LogDebug("No resource selected.");
                loadingTcs.TrySetResult();
            }

            _resourceSubscriptionTask = Task.Run(async () =>
            {
                await foreach (var changes in subscription.WithCancellation(_resourceSubscriptionToken).ConfigureAwait(false))
                {
                    foreach (var (changeType, resource) in changes)
                    {
                        await OnResourceChanged(changeType, resource);

                        // the initial snapshot we obtain is [almost] never correct (it's always empty)
                        // we still want to select the user's initial queried resource on page load,
                        // so if there is no selected resource when we
                        // receive an added resource, and that added resource name == ResourceName,
                        // we should mark it as selected
                        if (ResourceName is not null && PageViewModel.SelectedResource is null && changeType == ResourceViewModelChangeType.Upsert && string.Equals(ResourceName, resource.Name, StringComparisons.ResourceName))
                        {
                            SetSelectedResourceOption(resource);
                        }
                    }

                    await InvokeAsync(StateHasChanged);
                }
            });
        }

        void SetSelectedResourceOption(ResourceViewModel resource)
        {
            Debug.Assert(_resources is not null);

            PageViewModel.SelectedOption = _resources.Single(option => option.Id?.Type is not OtlpApplicationType.ResourceGrouping && string.Equals(ResourceName, option.Id?.InstanceId, StringComparison.Ordinal));
            PageViewModel.SelectedResource = resource;

            Logger.LogDebug("Selected console resource from name {ResourceName}.", ResourceName);
            loadingTcs.TrySetResult();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogDebug("Initializing console logs view model.");
        if (await this.InitializeViewModelAsync())
        {
            return;
        }

        var selectedResourceName = PageViewModel.SelectedResource?.Name;
        if (!string.Equals(selectedResourceName, _openApiSubscription?.Name, StringComparisons.ResourceName))
        {
            Logger.LogDebug("New resource {ResourceName} selected.", selectedResourceName);

            OpenApiSubscription? newOpenApiSubscription = null;
            if (selectedResourceName is not null)
            {
                newOpenApiSubscription = new OpenApiSubscription { Name = selectedResourceName };
                Logger.LogDebug("Creating new subscription {SubscriptionId}.", newOpenApiSubscription.SubscriptionId);

                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    newOpenApiSubscription.CancellationToken.Register(state =>
                    {
                        var s = (OpenApiSubscription)state!;
                        Logger.LogDebug("Canceling subscription {SubscriptionId} to {ResourceName}.", s.SubscriptionId, s.Name);
                    }, newOpenApiSubscription);
                }
            }

            if (_openApiSubscription is { } currentSubscription)
            {
                currentSubscription.Cancel();
                _openApiSubscription = newOpenApiSubscription;

                await TaskHelpers.WaitIgnoreCancelAsync(currentSubscription.SubscriptionTask);
            }
            else
            {
                _openApiSubscription = newOpenApiSubscription;
            }

            // OpenAPI
            await LoadOpenAPISpecification();
        }
    }

    private async Task OnResourceChanged(ResourceViewModelChangeType changeType, ResourceViewModel resource)
    {
        if (changeType == ResourceViewModelChangeType.Upsert)
        {
            _resourceByName[resource.Name] = resource;
            UpdateResourcesList();

            if (string.Equals(PageViewModel.SelectedResource?.Name, resource.Name, StringComparisons.ResourceName))
            {
                PageViewModel.SelectedResource = resource;
            }
        }
        else if (changeType == ResourceViewModelChangeType.Delete)
        {
            var removed = _resourceByName.TryRemove(resource.Name, out _);
            Debug.Assert(removed, "Cannot remove unknown resource.");

            if (string.Equals(PageViewModel.SelectedResource?.Name, resource.Name, StringComparisons.ResourceName))
            {
                // The selected resource was deleted
                PageViewModel.SelectedOption = _noSelection;
                await HandleSelectedOptionChangedAsync();
            }

            UpdateResourcesList();
        }
    }

    private void ParseOpenApiDocument(string baseUrl)
    {
        if (PageViewModel.Document == null)
        {
            return;
        }

        PageViewModel.Methods = new Dictionary<string, List<OpenApiViewModelMethod>>();

        foreach (var path in PageViewModel.Document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                List<OpenApiViewModelParameter> parameters = new List<OpenApiViewModelParameter>();

                foreach (var parameter in operation.Value.Parameters)
                {
                    parameters.Add(new OpenApiViewModelParameter
                    {
                        IsInPath = parameter.In == ParameterLocation.Path,
                        IsInQuery = parameter.In == ParameterLocation.Query,
                        IsRequired = parameter.Required,
                        Name = parameter.Name,
                        Type = string.IsNullOrEmpty(parameter.Schema.Format) ? parameter.Schema.Type : parameter.Schema.Format
                    });
                }

                var method = HttpMethod.Parse(operation.Key.ToString());
                var color = OpenApiUtils.GetBadgeColorFromHttpMethod(method);

                foreach (var tag in operation.Value.Tags)
                {
                    if (PageViewModel.Methods.TryGetValue(tag.Name, out var methods))
                    {
                        methods.Add(new OpenApiViewModelMethod
                        {
                            BadgeColor = color,
                            Description = path.Value.Description,
                            Id = $"{path.Key}-{operation.Key}".ToLower(),
                            Method = method,
                            Name = path.Key,
                            RequestParameters = parameters,
                            Url = baseUrl + path.Key
                        });
                    }
                    else
                    {
                        PageViewModel.Methods[tag.Name] = new List<OpenApiViewModelMethod>
                        {
                            new OpenApiViewModelMethod
                            {
                                BadgeColor = color,
                                Description = path.Value.Description,
                                Id = $"{path.Key}-{operation.Key}".ToLower(),
                                Method = method,
                                Name = path.Key,
                                RequestParameters = parameters,
                                Url = baseUrl + path.Key
                            }
                        };
                    }
                }
            }
        }
    }

    private async Task SendRequest()
    {
        if (PageViewModel.SelectedResource == null)
        {
            Logger.LogDebug("Cannot send request! No resource selected!");
            return;
        }

        if (PageViewModel.SelectedMethod == null)
        {
            Logger.LogDebug("Cannot send request! No method selected!");
            return;
        }

        var method = PageViewModel.SelectedMethod;
        method.Response = null;

        var url = method.Url;
        foreach (var parameter in method.RequestParameters)
        {
            if (string.IsNullOrEmpty(parameter.Value))
            {
                if (parameter.IsInPath)
                {
                    Logger.LogError("Cannot send request! Parameter {Name} is required!", parameter.Name);
                    return;
                }

                continue;
            }

            if (parameter.IsInPath)
            {
                url = url.Replace($"{{{parameter.Name}}}", Uri.EscapeDataString(parameter.Value));
            }
            else if (parameter.IsInQuery)
            {
                url = QueryHelpers.AddQueryString(url, parameter.Name, Uri.EscapeDataString(parameter.Value));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        method.Response = await HttpClient.SendAsync(new HttpRequestMessage
        {
            Method = method.Method,
            RequestUri = new Uri(url)
        });

        if (_response is not null)
        {
            await _response.UpdateResponse();
        }
    }

    private async Task StopAndClearOpenApiSubscriptionAsync()
    {
        if (_openApiSubscription is { } openApiSubscription)
        {
            openApiSubscription.Cancel();
            await TaskHelpers.WaitIgnoreCancelAsync(openApiSubscription.SubscriptionTask);

            _openApiSubscription = null;
        }
    }

    private void UpdateResourcesList()
    {
        _resources = GetConsoleLogResourceSelectViewModels(_resourceByName, _noSelection, Loc[nameof(Dashboard.Resources.OpenApi.OpenApiUnknownState)]);
    }

    public Task UpdateViewModelFromQueryAsync(OpenApiViewModel viewModel)
    {
        if (_resources is not null && ResourceName is not null)
        {
            var selectedOption = _resources.FirstOrDefault(c => string.Equals(ResourceName, c.Id?.InstanceId, StringComparisons.ResourceName)) ?? _noSelection;

            viewModel.SelectedOption = selectedOption;
            viewModel.SelectedResource = selectedOption.Id?.InstanceId is null ? null : _resourceByName[selectedOption.Id.InstanceId];
        }
        else
        {
            viewModel.SelectedOption = _noSelection;
            viewModel.SelectedResource = null;
        }

        return Task.CompletedTask;
    }

    public class OpenApiPageState
    {
        public string? SelectedResource { get; set; }
    }

    private sealed class OpenApiSubscription
    {
        private static int s_subscriptionId;

        private readonly CancellationTokenSource _cts = new();
        private readonly int _subscriptionId = Interlocked.Increment(ref s_subscriptionId);

        public required string Name { get; init; }
        public Task? SubscriptionTask { get; set; }

        public CancellationToken CancellationToken => _cts.Token;
        public int SubscriptionId => _subscriptionId;
        public void Cancel() => _cts.Cancel();
    }

    public class OpenApiViewModel
    {
        public required OpenApiDocument? Document { get; set; }
        public required Dictionary<string, List<OpenApiViewModelMethod>>? Methods { get; set; }
        public required SelectViewModel<ResourceTypeDetails> SelectedOption { get; set; }
        public OpenApiViewModelMethod? SelectedMethod { get; set; }
        public required ResourceViewModel? SelectedResource { get; set; }
        public FluentTreeItem? SelectedTreeItem { get; set; }
    }

    public class OpenApiViewModelParameter
    {
        public required bool IsInPath { get; set; }
        public required bool IsInQuery { get; set; }
        public required bool IsRequired { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string? Value { get; set; }
    }

    public class OpenApiViewModelMethod
    {
        public required string BadgeColor { get; set; }
        public required string Description { get; set; }
        public required string Id { get; set; }
        public required HttpMethod Method { get; set; }
        public required string Name { get; set; }
        public required List<OpenApiViewModelParameter> RequestParameters { get; set; }
        public HttpResponseMessage? Response { get; set; }
        public required string Url { get; set; }
    }

    internal static ImmutableList<SelectViewModel<ResourceTypeDetails>> GetConsoleLogResourceSelectViewModels(
        ConcurrentDictionary<string, ResourceViewModel> resourcesByName,
        SelectViewModel<ResourceTypeDetails> noSelectionViewModel,
        string resourceUnknownStateText)
    {
        var builder = ImmutableList.CreateBuilder<SelectViewModel<ResourceTypeDetails>>();

        foreach (var grouping in resourcesByName
            .Where(r => !r.Value.IsHiddenState())
            .OrderBy(c => c.Value, ResourceViewModelNameComparer.Instance)
            .GroupBy(r => r.Value.DisplayName, StringComparers.ResourceName))
        {
            string applicationName;

            if (grouping.Count() > 1)
            {
                applicationName = grouping.Key;

                builder.Add(new SelectViewModel<ResourceTypeDetails>
                {
                    Id = ResourceTypeDetails.CreateApplicationGrouping(applicationName, true),
                    Name = applicationName
                });
            }
            else
            {
                applicationName = grouping.First().Value.DisplayName;
            }

            foreach (var resource in grouping.Select(g => g.Value).OrderBy(r => r, ResourceViewModelNameComparer.Instance))
            {
                builder.Add(ToOption(resource, grouping.Count() > 1, applicationName));
            }
        }

        builder.Insert(0, noSelectionViewModel);
        return builder.ToImmutableList();

        SelectViewModel<ResourceTypeDetails> ToOption(ResourceViewModel resource, bool isReplica, string applicationName)
        {
            var id = isReplica
                ? ResourceTypeDetails.CreateReplicaInstance(resource.Name, applicationName)
                : ResourceTypeDetails.CreateSingleton(resource.Name, applicationName);

            return new SelectViewModel<ResourceTypeDetails>
            {
                Id = id,
                Name = GetDisplayText()
            };

            string GetDisplayText()
            {
                var resourceName = ResourceViewModel.GetResourceName(resource, resourcesByName);

                if (resource.HasNoState())
                {
                    return $"{resourceName} ({resourceUnknownStateText})";
                }

                if (resource.IsRunningState())
                {
                    return resourceName;
                }

                return $"{resourceName} ({resource.State})";
            }
        }
    }
}
