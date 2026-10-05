using Fargo.Application.Common;
using Fargo.Core.Barcodes;
using Fargo.Core.Informations;
using Fargo.Core.Security;
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
            schema.MinLength = Name.MinLength;
            schema.MaxLength = Name.MaxLength;
        }

        if (jsonTypeInfo == typeof(FirstName))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "string";
            schema.MinLength = FirstName.MinLength;
            schema.MaxLength = FirstName.MaxLength;
        }

        if (jsonTypeInfo == typeof(LastName))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "string";
            schema.MinLength = LastName.MinLength;
            schema.MaxLength = LastName.MaxLength;
        }

        if (jsonTypeInfo == typeof(Description))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "string";
            schema.MaxLength = Description.MaxLength;
        }

        if (jsonTypeInfo == typeof(Nameid))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "string";
            schema.MinLength = Nameid.MinLength;
            schema.MaxLength = Nameid.MaxLength;
            schema.Pattern = @"^[a-z0-9][a-z0-9._-]*[a-z0-9]$";
        }

        if (jsonTypeInfo == typeof(Password))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "password";
            schema.MinLength = Password.MinLength;
            schema.MaxLength = Password.MaxLength;
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
