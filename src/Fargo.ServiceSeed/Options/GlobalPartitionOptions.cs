using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceSeed.Options;

public sealed class GlobalPartitionOptions
{
    public const string SectionName = "GlobalPartition";

    [Required(AllowEmptyStrings = false)]
    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;
}
