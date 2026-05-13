using Fargo.Application.Partitions;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Identity;
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
/// Provides the audit origin used when persisting changes and events.
/// </summary>
public interface IAuditPrincipal
{
    /// <summary>
    /// Gets the actor identifier that should be written to audit columns.
    /// </summary>
    Guid ActorGuid
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
/// Represents the result of a successful authentication.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate API requests.
/// </param>
/// <param name="RefreshToken">
/// The refresh token used to obtain a new access token when the current one expires.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
/// <param name="IsAdmin">
/// Whether the authenticated user has administrator privileges.
/// </param>
/// <param name="PermissionActions">
/// The effective set of actions the user is permitted to perform,
/// combining direct permissions and those inherited from user groups.
/// </param>
/// <param name="PartitionAccesses">
/// The effective set of partition identifiers the user can access,
/// including inherited group accesses and all descendant partitions.
/// </param>
public record AuthResult(
        Token AccessToken,
        Token RefreshToken,
        DateTimeOffset ExpiresAt,
        bool IsAdmin,
        IReadOnlyCollection<ActionType> PermissionActions,
        IReadOnlyCollection<Guid> PartitionAccesses
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
/// Provides extension methods for <see cref="IAuthorizationContext"/> to enforce
/// authorization and partition-based access control rules.
/// </summary>
/// <remarks>
/// These methods centralize validation logic used across the application layer,
/// ensuring that authorization checks are consistently applied before executing
/// domain operations.
/// </remarks>
public static class AuthorizationContextExtensions
{
    extension(IAuthorizationContext context)
    {
        /// <summary>
        /// Ensures that the actor has permission to execute the specified action.
        /// </summary>
        /// <param name="action">
        /// The action that requires authorization.
        /// </param>
        /// <exception cref="UserNotAuthorizedFargoApplicationException">
        /// Thrown when the actor does not have permission to perform the specified action.
        /// </exception>
        /// <remarks>
        /// This validation enforces action-level authorization rules defined by the domain.
        /// </remarks>
        public void ValidateHasPermission(ActionType action)
        {
            if (!context.IsAdmin && !context.PermissionActions.Contains(action))
            {
                throw new UserNotAuthorizedFargoApplicationException(context.ActorGuid, action);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partition.
        /// </summary>
        /// <param name="partitionGuid">
        /// The unique identifier of the partition to validate access for.
        /// </param>
        /// <exception cref="PartitionAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified partition.
        /// </exception>
        /// <remarks>
        /// This validation enforces partition-level isolation, ensuring that actors
        /// can only interact with data within their authorized partitions.
        /// </remarks>
        public void ValidateHasPartitionAccess(Guid partitionGuid)
        {
            if (!context.IsAdmin && !context.PartitionAccesses.Contains(partitionGuid))
            {
                throw new PartitionAccessDeniedFargoApplicationException(partitionGuid, context.ActorGuid);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partitioned entity.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of the partitioned entity.
        /// </typeparam>
        /// <param name="partitioned">
        /// The entity whose access should be validated.
        /// </param>
        /// <exception cref="PartitionedEntityAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified entity.
        /// </exception>
        /// <remarks>
        /// This validation combines entity-level and partition-level checks,
        /// ensuring that the actor can access the given entity based on its
        /// partition associations.
        /// </remarks>
        public void ValidateHasAccess<TEntity>(TEntity partitioned)
            where TEntity : IEntity, IPartitionedEntity
        {
            if (!context.IsAdmin &&
                partitioned.Partitions.Count > 0 &&
                !partitioned.Partitions.Any(p => context.PartitionAccesses.Contains(p.Guid)))
            {
                throw new PartitionedEntityAccessDeniedFargoApplicationException(partitioned.Guid, context.ActorGuid);
            }
        }

    }
}

#endregion Authorization Helpers

