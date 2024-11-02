// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Utils;
using Microsoft.OpenApi.Readers;

namespace Aspire.Dashboard.Model;

public sealed class OpenApiRepository
{
    private readonly Dictionary<string, OpenApiModel> _models = [];
    private readonly HttpClient _httpClient;

    public OpenApiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<OpenApiModel?> CreateModelFromResource(ResourceViewModel resource)
    {
        var baseUrl = GetNonInternalUrlFromResource(resource);

        if (baseUrl == null)
        {
            return null;
        }

        var response = await _httpClient.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(baseUrl + "/openapi/v1.json")
        }).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var reader = new OpenApiStringReader();
        var document = reader.Read(await response.Content.ReadAsStringAsync().ConfigureAwait(false), out var diagnostic);

        var tags = new Dictionary<string, List<OpenApiMethod>>();

        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var httpMethod = HttpMethod.Parse(operation.Key.ToString());
                var color = OpenApiUtils.GetBadgeColorFromHttpMethod(httpMethod);

                foreach (var tag in operation.Value.Tags)
                {
                    var method = new OpenApiMethod
                    {
                        BadgeColor = color,
                        Description = path.Value.Description,
                        Id = $"{path.Key}-{operation.Key}".ToLower(),
                        MethodName = httpMethod.ToString(),
                        Path = path.Key
                    };

                    if (tags.TryGetValue(tag.Name, out var taggedMethods))
                    {
                        taggedMethods.Add(method);
                    }
                    else
                    {
                        tags[tag.Name] = [method];
                    }
                }
            }
        }

        return new OpenApiModel
        {
            Diagnostic = diagnostic,
            Document = document,
            Tags = tags
        };
    }

    public async Task<OpenApiModel?> GetModelFromResourceAsync(ResourceViewModel resource)
    {
        if (_models.TryGetValue(resource.Uid, out var model))
        {
            return model;
        }

        model = await CreateModelFromResource(resource).ConfigureAwait(false);
        _models[resource.Uid] = model!;
        return model;
    }

    private static string? GetNonInternalUrlFromResource(ResourceViewModel resource)
    {
        foreach (var url in resource.Urls)
        {
            if (url.IsInternal)
            {
                continue;
            }

            return url.Url.Scheme + "://" + url.Url.Host + ":" + url.Url.Port;
        }

        return null;
    }
}
