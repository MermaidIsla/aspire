//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Aspire.Microsoft.Data.SqlClient
{
    public sealed partial class MicrosoftDataSqlClientSettings
    {
        public string? ConnectionString { get { throw null; } set { } }

        public bool DisableHealthChecks { get { throw null; } set { } }

        public bool DisableTracing { get { throw null; } set { } }
    }
}

namespace Microsoft.Extensions.Hosting
{
    public static partial class AspireSqlServerSqlClientExtensions
    {
        public static void AddKeyedSqlServerClient(this IHostApplicationBuilder builder, string name, System.Action<Aspire.Microsoft.Data.SqlClient.MicrosoftDataSqlClientSettings>? configureSettings = null) { }

        public static void AddSqlServerClient(this IHostApplicationBuilder builder, string connectionName, System.Action<Aspire.Microsoft.Data.SqlClient.MicrosoftDataSqlClientSettings>? configureSettings = null) { }
    }
}