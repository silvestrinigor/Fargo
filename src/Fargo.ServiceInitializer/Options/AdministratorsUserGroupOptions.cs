using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceInitializer.Options;

public sealed class AdministratorsUserGroupOptions
{
    public const string SectionName = "Administrators";

    [Required(AllowEmptyStrings = false)]
    public required string Nameid { get; init; }

    public string Description { get; init; } = string.Empty;
}
