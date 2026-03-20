using Fargo.Application.Models.TreeModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.Infrastructure.Converters;

public sealed class NodeidJsonConverter : JsonConverter<Nodeid>
{
    public override Nodeid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Nodeid must be a string.");
        }

        var value = reader.GetString()!;

        try
        {
            return Nodeid.Parse(value, null);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid Nodeid format.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Nodeid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
