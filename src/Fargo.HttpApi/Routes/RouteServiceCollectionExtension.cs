namespace Fargo.HttpApi.Routes;

public static class FargoRouteServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureFargoRouteOptions()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["barcode"] = typeof(BarcodeRouteConstraint);
            });

            return services;
        }
    }
}
