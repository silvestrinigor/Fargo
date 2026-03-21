using Fargo.Infrastructure.Converters;
using System.Text.Json;

namespace Fargo.Web.Api;

internal static class FargoJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        options.Converters.Add(new NameJsonConverter());
        options.Converters.Add(new DescriptionJsonConverter());
        options.Converters.Add(new LimitJsonConverter());
        options.Converters.Add(new PageJsonConverter());
        options.Converters.Add(new NameidJsonConverter());
        options.Converters.Add(new PasswordJsonConverter());
        options.Converters.Add(new TokenJsonConverter());
        options.Converters.Add(new FirstNameJsonConverter());
        options.Converters.Add(new LastNameJsonConverter());

        return options;
    }
}
