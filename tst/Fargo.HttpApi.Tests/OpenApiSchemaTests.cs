using Fargo.HttpContracts;
using Microsoft.OpenApi;
using System.Text.Json.Schema;

namespace Fargo.HttpApi.Tests;

public sealed class OpenApiSchemaTests
{
    [Fact]
    public void ArticlePatchRequestJsonSchema_Should_NotThrow_ForOptionalFieldTimeSpan()
    {
        var options = FargoHttpJsonSerializerOptions.Create();

        var schema = options.GetJsonSchemaAsNode(typeof(ArticlePatchRequest));

        Assert.NotNull(schema);
    }

    [Fact]
    public void OptionalFieldTimeSpanOpenApiSchema_Should_MatchWireValue()
    {
        var schema = new OpenApiSchema();

        FargoOpenApiSchemaTransformers.Apply(
            schema,
            typeof(OptionalField<TimeSpan>),
            parameterType: null);

        Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Type);
        Assert.Equal(@"^-?(\d+\.)?\d{2}:\d{2}:\d{2}(\.\d{1,7})?$", schema.Pattern);
    }
}
