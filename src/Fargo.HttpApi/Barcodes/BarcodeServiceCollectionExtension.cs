namespace Fargo.HttpApi.Barcodes;

public static class BarcodeServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureBarcodeRouteConstraint()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["barcode"] = typeof(BarcodeRouteConstraint);
            });

            return services;
        }
    }
}
