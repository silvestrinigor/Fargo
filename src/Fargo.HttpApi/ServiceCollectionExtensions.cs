using Fargo.HttpApi.Articles;
using Fargo.Infrastructure.Converters;
using global::Fargo.HttpContracts;

namespace Fargo.HttpApi;

/// <summary>
/// Provides extension methods for configuring HTTP API services.
/// </summary>
public static class ServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures the JSON serialization options used by the HTTP API.
        ///
        /// Custom converters are registered to support application-specific
        /// value objects and strongly typed models during JSON serialization
        /// and deserialization.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
        public IServiceCollection ConfigureFargoHttpJsonOptions()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new NameJsonConverter());
                options.SerializerOptions.Converters.Add(new DescriptionJsonConverter());
                options.SerializerOptions.Converters.Add(new LimitJsonConverter());
                options.SerializerOptions.Converters.Add(new PageJsonConverter());
                options.SerializerOptions.Converters.Add(new TokenJsonConverter());
                options.SerializerOptions.Converters.Add(new FirstNameJsonConverter());
                options.SerializerOptions.Converters.Add(new LastNameJsonConverter());
                options.SerializerOptions.Converters.Add(new NameidJsonConverter());
                options.SerializerOptions.Converters.Add(new MassJsonConverter());
                options.SerializerOptions.Converters.Add(new LengthJsonConverter());
                options.SerializerOptions.Converters.Add(new DensityJsonConverter());
                options.SerializerOptions.Converters.Add(new ColorJsonConverter());
                options.SerializerOptions.Converters.Add(new Ean13JsonConverter());
                options.SerializerOptions.Converters.Add(new Ean8JsonConverter());
                options.SerializerOptions.Converters.Add(new UpcAJsonConverter());
                options.SerializerOptions.Converters.Add(new UpcEJsonConverter());
                options.SerializerOptions.Converters.Add(new Code128JsonConverter());
                options.SerializerOptions.Converters.Add(new Code39JsonConverter());
                options.SerializerOptions.Converters.Add(new Itf14JsonConverter());
                options.SerializerOptions.Converters.Add(new Gs1128JsonConverter());
                options.SerializerOptions.Converters.Add(new QrCodeJsonConverter());
                options.SerializerOptions.Converters.Add(new DataMatrixJsonConverter());
                options.SerializerOptions.Converters.Add(new OptionalValueJsonConverterFactory());
                options.SerializerOptions.Converters.Add(new OptionalFieldJsonConverterFactory());
            });

            return services;
        }

        /// <summary>
        /// Configures runtime routing options used by HTTP API endpoints.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
        public IServiceCollection ConfigureFargoRouting()
        {
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["barcode"] = typeof(ArticleBarcodeRouteConstraint);
            });

            return services;
        }
    }
}
