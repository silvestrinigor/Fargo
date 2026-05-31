using System.Text.Json;

namespace Fargo.HttpClient;

public static class FargoHttpJsonSerializerOptions
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);

        return options;
    }
}
