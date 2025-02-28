//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Aspire.Hosting
{
    public static partial class AzureWebPubSubExtensions
    {
        public static ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubResource> AddAzureWebPubSub(this IDistributedApplicationBuilder builder, string name) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubHubResource> AddEventHandler(this ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubHubResource> builder, ApplicationModel.ReferenceExpression urlExpression, string userEventPattern = "*", string[]? systemEvents = null, global::Azure.Provisioning.WebPubSub.UpstreamAuthSettings? authSettings = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubHubResource> AddEventHandler(this ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubHubResource> builder, ApplicationModel.ReferenceExpression.ExpressionInterpolatedStringHandler urlTemplateExpression, string userEventPattern = "*", string[]? systemEvents = null, global::Azure.Provisioning.WebPubSub.UpstreamAuthSettings? authSettings = null) { throw null; }

        public static ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubHubResource> AddHub(this ApplicationModel.IResourceBuilder<ApplicationModel.AzureWebPubSubResource> builder, string hubName) { throw null; }
    }
}

namespace Aspire.Hosting.ApplicationModel
{
    public partial class AzureWebPubSubHubResource : Resource, IResourceWithParent<AzureWebPubSubResource>, IResourceWithParent, IResource
    {
        public AzureWebPubSubHubResource(string name, AzureWebPubSubResource webpubsub) : base(default!) { }

        public AzureWebPubSubResource Parent { get { throw null; } }
    }

    public partial class AzureWebPubSubResource : Azure.AzureProvisioningResource, IResourceWithConnectionString, IResource, IManifestExpressionProvider, IValueProvider, IValueWithReferences
    {
        public AzureWebPubSubResource(string name, System.Action<Azure.AzureResourceInfrastructure> configureInfrastructure) : base(default!, default!) { }

        public ReferenceExpression ConnectionStringExpression { get { throw null; } }

        public Azure.BicepOutputReference Endpoint { get { throw null; } }
    }
}