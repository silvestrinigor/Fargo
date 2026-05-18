using Fargo.Application;
using Fargo.HttpApi.Articles;
using Fargo.Infrastructure.Converters;
using Fargo.Sdk.Contracts.Articles;
using System.Text.Json.Nodes;

namespace Fargo.HttpApi.Extensions;

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

        /// <summary>
        /// Adds OpenAPI configuration including schema transformations
        /// for custom pagination value objects such as <see cref="Page"/> and <see cref="Limit"/>.
        /// </summary>
        /// <returns>
        /// The same <see cref="IServiceCollection"/> instance so that additional
        /// calls can be chained.
        /// </returns>
        public IServiceCollection AddFargoOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, context, _) =>
                {
                    if (context.ParameterDescription?.Type == typeof(ArticleBarcode))
                    {
                        schema.Type = Microsoft.OpenApi.JsonSchemaType.String;
                        schema.Pattern = @".+:(Ean13|Ean8|UpcA|UpcE|Code128|Code39|Itf14|Gs1128|QrCode|DataMatrix)$";
                        schema.Example = JsonValue.Create("7891234567895:Ean13");
                    }

                    if (context.ParameterDescription?.Type == typeof(Page?))
                    {
                        schema.Type = Microsoft.OpenApi.JsonSchemaType.Integer;
                        schema.Minimum = Page.MinValue.ToString();
                        schema.Default = JsonValue.Create(Page.FirstPage.Value);
                    }

                    if (context.ParameterDescription?.Type == typeof(Limit?))
                    {
                        schema.Type = Microsoft.OpenApi.JsonSchemaType.Integer;
                        schema.Minimum = Limit.MinValue.ToString();
                        schema.Maximum = Limit.MaxValue.ToString();
                        schema.Default = JsonValue.Create(Limit.MaxLimit.Value);
                    }

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
