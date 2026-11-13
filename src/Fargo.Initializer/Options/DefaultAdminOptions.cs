using System.ComponentModel.DataAnnotations;

namespace Fargo.Initializer.Options;

/// <summary>
/// Used only during system initialization to create the default administrator; it does not affect
/// authentication afterwards.
/// </summary>
public sealed class DefaultAdminOptions
{
    public const string SectionName = "DefaultAdmin";

    [Required(AllowEmptyStrings = false)]
    public required string Nameid { get; init; }

    /// <summary>
    /// Only used when the administrator account is created automatically; change it after the first deployment.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string Password { get; init; }

    public string Description { get; init; } = string.Empty;
}
