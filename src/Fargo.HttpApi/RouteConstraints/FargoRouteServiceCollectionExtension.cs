namespace Fargo.HttpApi.RouteConstraints;

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
