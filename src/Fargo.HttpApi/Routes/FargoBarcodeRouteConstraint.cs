using Fargo.Core.Shared.Barcodes;
using System.Globalization;

namespace Fargo.HttpApi.Routes;

public sealed class FargoBarcodeRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
        => values.TryGetValue(routeKey, out var value) &&
            Barcode.TryParse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture, out _);
}
