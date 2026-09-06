using Fargo.Application.Common;
using Fargo.Core.Barcodes;
using Fargo.Core.Informations;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

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
                    if (context.JsonTypeInfo.Type == typeof(Name))
                    {
                        schema.Type = JsonSchemaType.String;
                        schema.Format = "string";
                    }

                    FargoOpenApiSchemaTransformers.Apply(
                        schema,
                        context.ParameterDescription?.Type);

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}

internal static class FargoOpenApiSchemaTransformers
{
    public static void Apply(
        OpenApiSchema schema,
        Type? parameterType)
    {
        if (parameterType == typeof(Barcode))
        {
            schema.Type = JsonSchemaType.String;
            schema.Pattern = @".+:(ean13)$";
            schema.Example = JsonValue.Create("7891234567895:ean13");
        }

        if (parameterType == typeof(Page?))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Minimum = Page.MinValue.ToString();
            schema.Default = JsonValue.Create(Page.FirstPage.Value);
        }

        if (parameterType == typeof(Limit?))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Minimum = Limit.MinValue.ToString();
            schema.Maximum = Limit.MaxValue.ToString();
            schema.Default = JsonValue.Create(Limit.MaxLimit.Value);
        }
    }
}
