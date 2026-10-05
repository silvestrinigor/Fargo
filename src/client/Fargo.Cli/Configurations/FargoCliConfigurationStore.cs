using System.Text.Json;

namespace Fargo.Cli.Configurations;

public sealed class FargoCliConfigurationStore : IFargoCliConfigurationStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public FargoCliConfigurationStore()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "Fargo");

        _filePath = Path.Combine(
            configDirectory,
            "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public FargoCliConfiguration Load()
    {
        if (!File.Exists(_filePath))
        {
            return new FargoCliConfiguration
            {
                Server = new Uri("http://localhost:5000")
            };
        }

        var json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<FargoCliConfiguration>(
                   json,
                   _jsonOptions)
               ?? throw new InvalidOperationException(
                   "Fargo configuration is invalid.");
    }

    public void Save(FargoCliConfiguration configuration)
    {
        var directory = Path.GetDirectoryName(_filePath)!;

        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(
            configuration,
            _jsonOptions);

        File.WriteAllText(_filePath, json);
    }
}
