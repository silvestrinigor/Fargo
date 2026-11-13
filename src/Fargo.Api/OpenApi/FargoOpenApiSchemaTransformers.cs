using Fargo.Application.Common.Paging;
using Fargo.Core.Barcodes;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;
using Microsoft.OpenApi;
using System.Drawing;
using System.Text.Json.Nodes;
using UnitsNet;

namespace Fargo.Api.OpenApi;

public static class FargoOpenApiSchemaTransformers
{
    /// <summary>
    /// Unwraps <see cref="Nullable{T}"/> first, so <c>Name</c> and <c>Name?</c> resolve to the same rules.
    /// </summary>
    public static void ApplyFargoTypes(OpenApiSchema schema, Type? jsonTypeInfo)
    {
        var type = jsonTypeInfo is null
            ? null
            : Nullable.GetUnderlyingType(jsonTypeInfo) ?? jsonTypeInfo;

        if (type == typeof(Name))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = Name.MinLength;
            schema.MaxLength = Name.MaxLength;
        }

        if (type == typeof(FirstName))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = FirstName.MinLength;
            schema.MaxLength = FirstName.MaxLength;
        }

        if (type == typeof(LastName))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = LastName.MinLength;
            schema.MaxLength = LastName.MaxLength;
        }

        if (type == typeof(Description))
        {
            schema.Type = JsonSchemaType.String;
            schema.MaxLength = Description.MaxLength;
        }

        if (type == typeof(Nameid))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = Nameid.MinLength;
            schema.MaxLength = Nameid.MaxLength;
            schema.Pattern = @"^[a-z0-9][a-z0-9._-]*[a-z0-9]$";
        }

        if (type == typeof(Password))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "password";
            schema.MinLength = Password.MinLength;
            schema.MaxLength = Password.MaxLength;
        }

        if (type == typeof(Token))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = Token.MinLength;
            schema.MaxLength = Token.MaxLength;
        }

        if (type == typeof(Ean13))
        {
            schema.Type = JsonSchemaType.String;
            schema.MinLength = Ean13.CodeLength;
            schema.MaxLength = Ean13.CodeLength;
            schema.Pattern = @"^\d{13}$";
        }

        // Serialized as a CSS hex color string, e.g. "#RRGGBB".

        if (type == typeof(Color))
        {
            schema.Type = JsonSchemaType.String;
            schema.Pattern = @"^#[0-9a-fA-F]{6}$";
            schema.Example = JsonValue.Create("#FFFFFF");
        }

        // UnitsNet quantities are serialized as { "value": number, "unit": string }.

        if (type == typeof(Mass))
        {
            schema.Type = JsonSchemaType.Object;
            schema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["value"] = new OpenApiSchema { Type = JsonSchemaType.Number },
                ["unit"] = new OpenApiSchema { Type = JsonSchemaType.String, Example = JsonValue.Create("g") }
            };
            schema.Required = new HashSet<string> { "value", "unit" };
        }

        if (type == typeof(Length))
        {
            schema.Type = JsonSchemaType.Object;
            schema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["value"] = new OpenApiSchema { Type = JsonSchemaType.Number },
                ["unit"] = new OpenApiSchema { Type = JsonSchemaType.String, Example = JsonValue.Create("m") }
            };
            schema.Required = new HashSet<string> { "value", "unit" };
        }

        // Scalar is a plain number (default unit) or a "<value> <unit>" string.

        if (type == typeof(Scalar))
        {
            schema.OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaType.Number },
                new OpenApiSchema { Type = JsonSchemaType.String }
            ];
        }
    }

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
