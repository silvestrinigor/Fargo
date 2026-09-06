using Fargo.Application.Common;
using Fargo.Core.Barcodes;
using Fargo.Core.Informations;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Fargo.Http.OpenApi;

/// <summary>
/// Provides static methods for applying custom schema transformations to OpenAPI specifications
/// for specific types used in the Fargo application.
/// </summary>
public static class FargoOpenApiSchemaTransformers
{
    /// <summary>
    /// Applies type-specific schema transformations to OpenAPI schemas.
    /// This method modifies the schema based on the provided JSON type information.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to be modified</param>
    /// <param name="jsonTypeInfo">The type information used to determine schema modifications</param>
    public static void ApplyFargoTypes(OpenApiSchema schema, Type? jsonTypeInfo)
    {
        if (jsonTypeInfo == typeof(Name))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "string";
        }
    }

    /// <summary>
    /// Applies parameter-specific schema transformations to OpenAPI schemas.
    /// This method modifies the schema based on the provided parameter type.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to be modified</param>
    /// <param name="parameterType">The parameter type used to determine schema modifications</param>
    public static void ApplyFargoParameters(OpenApiSchema schema, Type? parameterType)
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
