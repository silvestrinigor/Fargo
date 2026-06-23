using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Identity;
using Fargo.Core.Users;

namespace Fargo.Application.Identity;

#region Contracts

public interface ICurrentActor
{
    ActorId ActorId
    {
        get;
    }

    bool IsAuthenticated
    {
        get;
    }
}

/// <summary>
/// Defines a service responsible for generating access tokens
/// for authenticated users.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user for whom the token will be generated.
    /// </param>
    /// <returns>
    /// A <see cref="TokenGenerateResult"/> containing the generated
    /// access token and its expiration information.
    /// </returns>
    TokenGenerateResult Generate(User user);
}

/// <summary>
/// Represents the result of generating an access token.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate requests.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
public record TokenGenerateResult(
        Token AccessToken,
        DateTimeOffset ExpiresAt
        );

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

#endregion Contracts

