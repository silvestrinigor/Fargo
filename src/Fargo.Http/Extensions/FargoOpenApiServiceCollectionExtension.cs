using Fargo.Application.Common;
using Fargo.Http.OpenApi;

namespace Fargo.Http.Extensions;

public static class FargoOpenApiServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds OpenAPI configuration including schema transformations
        /// for custom pagination value objects such as <see cref="Page"/> and <see cref="Limit"/>.
        /// </summary>
        public IServiceCollection AddFargoOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, context, _) =>
                {
                    FargoOpenApiSchemaTransformers.ApplyFargoParameters(schema, context.ParameterDescription?.Type);

                    FargoOpenApiSchemaTransformers.ApplyFargoTypes(schema, context.JsonTypeInfo.Type);

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
