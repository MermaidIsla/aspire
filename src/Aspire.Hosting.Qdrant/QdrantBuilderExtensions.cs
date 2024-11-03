// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Qdrant;
using Aspire.Hosting.Utils;
using Aspire.Qdrant.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Qdrant resources to the application model.
/// </summary>
public static class QdrantBuilderExtensions
{
    private const int QdrantPortGrpc = 6334;
    private const int QdrantPortHttp = 6333;
    private const string ApiKeyEnvVarName = "QDRANT__SERVICE__API_KEY";
    private const string EnableStaticContentEnvVarName = "QDRANT__SERVICE__ENABLE_STATIC_CONTENT";

    /// <summary>
    /// Adds a Qdrant resource to the application. A container is used for local development.
    /// </summary>
    /// <remarks>
    /// The .NET client library uses the gRPC port by default to communicate and this resource exposes that endpoint.
    /// This version of the package defaults to the <inheritdoc cref="QdrantContainerImageTags.Tag"/> tag of the <inheritdoc cref="QdrantContainerImageTags.Image"/> container image.
    /// </remarks>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency</param>
    /// <param name="apiKey">The parameter used to provide the API Key for the Qdrant resource. If <see langword="null"/> a random key will be generated as {name}-Key.</param>
    /// <param name="grpcPort">The host port of gRPC endpoint of Qdrant database.</param>
    /// <param name="httpPort">The host port of HTTP endpoint of Qdrant database.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{QdrantServerResource}"/>.</returns>
    public static IResourceBuilder<QdrantServerResource> AddQdrant(this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<ParameterResource>? apiKey = null,
        int? grpcPort = null,
        int? httpPort = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var apiKeyParameter = apiKey?.Resource ??
            ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-Key", special: false);
        var qdrant = new QdrantServerResource(name, apiKeyParameter);

        HttpClient? httpClient = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(qdrant, async (@event, ct) =>
        {
            var connectionString = await qdrant.HttpConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false)
            ?? throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{qdrant.Name}' resource but the connection string was null.");
            httpClient = CreateQdrantHttpClient(connectionString);
        });

        var healthCheckKey = $"{name}_check";
        builder.Services.AddHealthChecks()
          .Add(new HealthCheckRegistration(
              healthCheckKey,
              sp => new QdrantHealthCheck(httpClient!),
              failureStatus: default,
              tags: default,
              timeout: default));

        return builder.AddResource(qdrant)
            .WithImage(QdrantContainerImageTags.Image, QdrantContainerImageTags.Tag)
            .WithImageRegistry(QdrantContainerImageTags.Registry)
            .WithHttpEndpoint(port: grpcPort, targetPort: QdrantPortGrpc, name: QdrantServerResource.PrimaryEndpointName)
            .WithEndpoint(QdrantServerResource.PrimaryEndpointName, endpoint =>
            {
                endpoint.Transport = "http2";
            })
            .WithHttpEndpoint(port: httpPort, targetPort: QdrantPortHttp, name: QdrantServerResource.HttpEndpointName)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables[ApiKeyEnvVarName] = qdrant.ApiKeyParameter;

                // If in Publish mode, disable static content, which disables the Dashboard Web UI
                // https://github.com/qdrant/qdrant/blob/acb04d5f0d22b46a756b31c0fc507336a0451c15/src/settings.rs#L36-L40
                if (builder.ExecutionContext.IsPublishMode)
                {
                    context.EnvironmentVariables[EnableStaticContentEnvVarName] = "0";
                }
            })
            .WithHealthCheck(healthCheckKey);
    }

    /// <summary>
    /// Adds a named volume for the data folder to a Qdrant container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the resource name.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<QdrantServerResource> WithDataVolume(this IResourceBuilder<QdrantServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), "/qdrant/storage",
            isReadOnly);
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a Qdrant container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<QdrantServerResource> WithDataBindMount(this IResourceBuilder<QdrantServerResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder.WithBindMount(source, "/qdrant/storage", isReadOnly);
    }

    /// <summary>
    /// Add a reference to a Qdrant server to the resource.
    /// </summary>
    /// <param name="builder">An <see cref="IResourceBuilder{T}"/> for <see cref="ProjectResource"/></param>
    /// <param name="qdrantResource">The Qdrant server resource</param>
    /// <returns></returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<QdrantServerResource> qdrantResource)
         where TDestination : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(qdrantResource);

        builder.WithEnvironment(context =>
        {
            // primary endpoint (gRPC)
            context.EnvironmentVariables[$"ConnectionStrings__{qdrantResource.Resource.Name}"] = qdrantResource.Resource.ConnectionStringExpression;

            // HTTP endpoint
            context.EnvironmentVariables[$"ConnectionStrings__{qdrantResource.Resource.Name}_{QdrantServerResource.HttpEndpointName}"] = qdrantResource.Resource.HttpConnectionStringExpression;
        });

        return builder;
    }

    private static HttpClient CreateQdrantHttpClient(string? connectionString)
    {
        if (connectionString is null)
        {
            throw new InvalidOperationException("Connection string is unavailable");
        }

        Uri? endpoint = null;
        string? key = null;

        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
        {
            endpoint = uri;
        }
        else
        {
            var connectionBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (connectionBuilder.TryGetValue("Endpoint", out var endpointValue) && Uri.TryCreate(endpointValue.ToString(), UriKind.Absolute, out var serviceUri))
            {
                endpoint = serviceUri;
            }

            if (connectionBuilder.TryGetValue("Key", out var keyValue))
            {
                key = keyValue.ToString();
            }
        }

        var client = new HttpClient();
        client.BaseAddress = endpoint;
        if (key is not null)
        {
            client.DefaultRequestHeaders.Add("Api-Key", key);
        }
        return client;
    }
}
