using Fargo.Domain.Users;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Provides mapping utilities for transforming <see cref="UserGroup"/> entities
/// into lightweight data transfer representations.
/// </summary>
/// <remarks>
/// This class centralizes projection logic for <see cref="UserGroup"/> to ensure
/// consistency across queries and in-memory transformations.
///
/// Two mapping approaches are provided:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="InformationProjection"/>: An expression-based projection intended
/// for use with LINQ providers (e.g., Entity Framework Core).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="ToInformation(UserGroup)"/>: An in-memory mapping method used when
/// the entity has already been materialized.
/// </description>
/// </item>
/// </list>
///
/// <para>
/// <strong>Important:</strong>
/// The projection includes permissions, which are mapped into value objects.
/// When using this projection with Entity Framework Core, ensure that the
/// permissions navigation property is properly handled, as some LINQ providers
/// may have limitations when projecting nested collections.
/// </para>
/// </remarks>
public static class UserGroupMappings
{
    /// <summary>
    /// Gets an expression used to project a <see cref="UserGroup"/> entity
    /// into a <see cref="UserGroupInformation"/> object.
    /// </summary>
    /// <remarks>
    /// This projection is intended for use in LINQ queries executed by a query provider
    /// such as Entity Framework Core. Because it is an expression tree, it can be
    /// translated into SQL, allowing only the required fields to be selected from
    /// the database.
    ///
    /// The projection includes the group's permissions, which are mapped into
    /// <see cref="Permission"/> value objects.
    ///
    /// <para>
    /// Depending on the LINQ provider and query shape, projecting nested collections
    /// (such as permissions) may result in client-side evaluation or require additional
    /// query configuration (e.g., proper joins or includes).
    /// </para>
    /// </remarks>
    public static readonly Expression<Func<UserGroup, UserGroupInformation>> InformationProjection =
        u => new UserGroupInformation(
            u.Guid,
            u.Nameid,
            u.Description,
            u.IsActive,
            u.Permissions.Select(p => new Permission(p.Guid, p.Action)).ToList()
        );

    /// <summary>
    /// Maps a <see cref="UserGroup"/> entity to a <see cref="UserGroupInformation"/> object.
    /// </summary>
    /// <param name="u">The <see cref="UserGroup"/> to map.</param>
    /// <returns>
    /// A new <see cref="UserGroupInformation"/> instance containing the mapped data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="u"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method performs an in-memory transformation and should be used only when
    /// the <see cref="UserGroup"/> entity has already been loaded into memory.
    ///
    /// The permissions collection is fully enumerated and mapped into
    /// <see cref="Permission"/> value objects.
    ///
    /// For queryable scenarios (e.g., database queries), prefer using
    /// <see cref="InformationProjection"/> to ensure efficient execution.
    /// </remarks>
    public static UserGroupInformation ToInformation(this UserGroup u)
    {
        ArgumentNullException.ThrowIfNull(u);

        return new UserGroupInformation(
            u.Guid,
            u.Nameid,
            u.Description,
            u.IsActive,
            [.. u.Permissions.Select(p => new Permission(p.Guid, p.Action))]
        );
    }
}
