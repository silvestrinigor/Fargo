using Fargo.HttpApi.Routes;

namespace Fargo.HttpApi.Extensions;

public static class FargoRouteServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoRouteConstraints()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["barcode"] = typeof(FargoBarcodeRouteConstraint);
            });

            return services;
        }
    }
}
