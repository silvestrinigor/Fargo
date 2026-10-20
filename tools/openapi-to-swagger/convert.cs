#:package Microsoft.OpenApi@2.0.0

// Converts an OpenAPI 3.x document to a Swagger 2.0 document so DocFX (which
// only consumes Swagger 2.0) can generate the REST API reference.
//
// Usage:
//   dotnet run tools/openapi-to-swagger/convert.cs -- <input-openapi.json> <output.swagger.json>
//
// Run it after regenerating src/server/Fargo.ServiceHttp/Fargo.ServiceHttp.json
// to refresh docs/web-api/reference/fargo.swagger.json.

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

if (args.Length != 2)
{
    Console.Error.WriteLine(
        "Usage: convert <input-openapi.json> <output.swagger.json>");
    return 1;
}

var inputPath = args[0];
var outputPath = args[1];

// Use an empty validation rule set so strict rules that do not apply to our
// downconversion (for example the path-signature uniqueness rule that trips on
// /articles/{articleGuid} vs /articles/{articleBarcode}) do not block loading.
var readerSettings = new OpenApiReaderSettings
{
    RuleSet = new ValidationRuleSet(),
};

var (document, diagnostic) = await OpenApiDocument.LoadAsync(inputPath, readerSettings);

if (diagnostic?.Errors is { Count: > 0 })
{
    foreach (var error in diagnostic.Errors)
    {
        Console.Error.WriteLine($"Parse error: {error.Message}");
    }

    return 1;
}

await using var streamWriter = new StreamWriter(outputPath);
var writer = new OpenApiJsonWriter(streamWriter);

document.SerializeAsV2(writer);

Console.WriteLine($"Converted '{inputPath}' to Swagger 2.0 at '{outputPath}'.");

return 0;
