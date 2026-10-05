namespace Fargo.ServiceSeed.Options;

public sealed class GlobalPartitionOptions
{
    public const string SectionName = "GlobalPartition";

    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;
}
