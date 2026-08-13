using Fargo.HttpApi.Shared.JsonConverters;
using System.Text.Json;

namespace Fargo.HttpApi.Shared.Extensions;

public static class FargoJsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddFargoJsonConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new NameJsonConverter());
        options.Converters.Add(new PasswordJsonConverter());
        options.Converters.Add(new DescriptionJsonConverter());
        options.Converters.Add(new LimitJsonConverter());
        options.Converters.Add(new PageJsonConverter());
        options.Converters.Add(new TokenJsonConverter());
        options.Converters.Add(new FirstNameJsonConverter());
        options.Converters.Add(new LastNameJsonConverter());
        options.Converters.Add(new NameidJsonConverter());
        options.Converters.Add(new MassJsonConverter());
        options.Converters.Add(new LengthJsonConverter());
        options.Converters.Add(new DensityJsonConverter());
        options.Converters.Add(new ColorJsonConverter());
        options.Converters.Add(new Ean13JsonConverter());
        options.Converters.Add(new ScalarJsonConverter());

        return options;
    }
}
