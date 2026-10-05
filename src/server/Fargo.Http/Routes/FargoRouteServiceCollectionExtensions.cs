namespace Fargo.Http.Routes;

public static class FargoRouteServiceCollectionExtensions
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
