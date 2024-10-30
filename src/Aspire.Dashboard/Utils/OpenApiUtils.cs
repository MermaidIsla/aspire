// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Dashboard.Utils;

public static class OpenApiUtils
{
    private const string HttpMethodDefaultColor = "gray";
    private const string HttpMethodDeleteColor = "red";
    private const string HttpMethodGetColor = "cornflowerblue";
    private const string HttpMethodPostColor = "green";
    private const string HttpMethodPutColor = "darkorange";

    public static string GetBadgeColorFromHttpMethod(HttpMethod method)
    {
        if (method == HttpMethod.Delete)
        {
            return HttpMethodDeleteColor;
        }

        if (method == HttpMethod.Get)
        {
            return HttpMethodGetColor;
        }

        if (method == HttpMethod.Post)
        {
            return HttpMethodPostColor;
        }

        if (method == HttpMethod.Put)
        {
            return HttpMethodPutColor;
        }

        return HttpMethodDefaultColor;
    }
}
