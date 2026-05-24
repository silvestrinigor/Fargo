using Fargo.Application;
using Fargo.Core.Barcodes;
using System.Text.Json.Nodes;

namespace Fargo.HttpApi;

public static class OpenApiServiceCollectionExtensions
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
                    if (context.ParameterDescription?.Type == typeof(Barcode))
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
