namespace Fargo.Application.System;

public sealed class GlobalPartitionOptions
{
    public const string SectionName = "GlobalPartition";

    public required string Name { get; init; }
}
