namespace Fargo.HttpApi.Routes;

public static class FargoRouteServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoRoutes()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["barcode"] = typeof(FargoBarcodeRouteConstraint);
            });

            return services;
        }
    }
}
