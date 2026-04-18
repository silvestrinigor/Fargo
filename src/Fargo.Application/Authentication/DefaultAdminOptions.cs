namespace Fargo.Application.Authentication;

/// <summary>
/// Configuration options used to create the default administrator
/// account during system initialization.
/// </summary>
/// <remarks>
/// These settings are used by the system initialization process to
/// create a default administrator user when no users exist in the system.
///
/// The values are typically provided through application configuration
/// (for example <c>appsettings.json</c> or environment variables).
///
/// This configuration is only used during system initialization and
/// does not affect authentication after the administrator account
/// has been created.
/// </remarks>
public sealed class DefaultAdminOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "DefaultAdmin";

    /// <summary>
    /// Gets the identifier (NAMEID) of the default administrator account.
    /// </summary>
    /// <remarks>
    /// This value defines the unique login identifier that will be assigned
    /// to the administrator user created during system initialization.
    /// </remarks>
    public required string Nameid { get; init; }

    /// <summary>
    /// Gets the password of the default administrator account.
    /// </summary>
    /// <remarks>
    /// This password is used only when the administrator account is
    /// automatically created during system initialization.
    ///
    /// It should be replaced or changed immediately after the system
    /// is first deployed to ensure proper security.
    /// </remarks>
    public required string Password { get; init; }
}
