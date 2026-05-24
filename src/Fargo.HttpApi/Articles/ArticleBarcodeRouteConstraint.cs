using Fargo.Core.Barcodes;
using System.Globalization;

namespace Fargo.HttpApi.Articles;

public sealed class ArticleBarcodeRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
        => values.TryGetValue(routeKey, out var value) &&
            Barcode.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, out _);
}
