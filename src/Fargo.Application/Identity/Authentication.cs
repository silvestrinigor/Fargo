using Fargo.Core.Actors;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Identity;
using Fargo.Core.Users;

namespace Fargo.Application.Identity;

#region Contracts

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
/// <remarks>
/// This abstraction allows the application layer to access
/// user identity information without depending on HTTP,
/// authentication frameworks, or infrastructure concerns.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Guid.Empty"/> when the user is not authenticated.
    /// </remarks>
    Guid UserGuid
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated
    {
        get;
    }
}

/// <summary>
/// Represents the resolved authorization snapshot for a request or user.
/// </summary>
public interface IAuthorizationContext
{
    Guid ActorGuid { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    IReadOnlyCollection<ActionType> PermissionActions { get; }

    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    IReadOnlyCollection<Guid> UserGroupGuids { get; }
}

/// <summary>
/// Immutable authorization snapshot used across the application layer.
/// </summary>
public sealed record AuthorizationContext(
    Guid ActorGuid,
    bool IsAuthenticated,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    IReadOnlyCollection<Guid> UserGroupGuids
) : IAuthorizationContext;

/// <summary>
/// Creates authorization snapshots from user data.
/// </summary>
public interface IAuthorizationContextFactory
{
    Task<IAuthorizationContext> CreateFromUserGuid(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    Task<IAuthorizationContext> CreateFromUser(
        User user,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides the authorization snapshot for the current request.
/// </summary>
public interface ICurrentAuthorizationContext
{
    Task<IAuthorizationContext> GetAsync(
        CancellationToken cancellationToken = default);
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

#region Authorization Helpers

/// <summary>
/// Provides conversion helpers from application authorization snapshots
/// to domain actors.
/// </summary>
public static class AuthorizationContextExtensions
{
    public static Actor ToActor(this IAuthorizationContext context)
        => new(
            context.ActorGuid,
            context.IsAdmin,
            context.IsAuthenticated,
            context.PermissionActions,
            context.PartitionAccesses);
}

#endregion Authorization Helpers
