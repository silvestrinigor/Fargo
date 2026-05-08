using Fargo.Sdk.Contracts.Articles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace Fargo.Api.Articles;

public sealed class ArticleBarcodeRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
        => values.TryGetValue(routeKey, out var value) &&
            ArticleBarcode.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, out _);
}
