using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fargo.HttpContracts;

public sealed record UnitValueDto(
    double Value,
    string Unit
);

public sealed record PermissionDto(
    Guid Guid,
    ActionType Action
);

public sealed record FargoProblemDetailsDto
{
    public int Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Instance { get; init; } = string.Empty;

    public string TraceId { get; init; } = string.Empty;
}

public readonly record struct OptionalField<TValue>(
    bool IsSpecified,
    TValue? Value)
    where TValue : struct
{
    public static OptionalField<TValue> FromValue(TValue? value) => new(true, value);
}

public sealed class OptionalFieldJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType &&
           typeToConvert.GetGenericTypeDefinition() == typeof(OptionalField<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalFieldJsonConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class OptionalFieldJsonConverter<TValue> : JsonConverter<OptionalField<TValue>>
    where TValue : struct
{
    public override OptionalField<TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return OptionalField<TValue>.FromValue(default);
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return OptionalField<TValue>.FromValue(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        OptionalField<TValue> value,
        JsonSerializerOptions options)
    {
        if (!value.IsSpecified)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

public static class FargoHttpJsonSerializerOptions
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        options.Converters.Add(new OptionalFieldJsonConverterFactory());

        return options;
    }
}
