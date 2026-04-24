using Fargo.Domain.Users;
using System.Linq.Expressions;

namespace Fargo.Application.Users;

/// <summary>
/// Provides mapping utilities for transforming <see cref="User"/> entities
/// into lightweight data transfer representations.
/// </summary>
/// <remarks>
/// This class centralizes projection logic for <see cref="User"/> to ensure
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
/// <see cref="ToInformation(User)"/>: An in-memory mapping method used when
/// the entity has already been materialized.
/// </description>
/// </item>
/// </list>
///
/// <para>
/// <strong>Important:</strong>
/// This projection includes nested collections (permissions and partition accesses).
/// Depending on the LINQ provider and query shape, this may require careful usage
/// to avoid client-side evaluation or inefficient queries.
/// </para>
/// </remarks>
public static class UserMappings
{
    /// <summary>
    /// Gets an expression used to project a <see cref="User"/> entity
    /// into a <see cref="UserInformation"/> object.
    /// </summary>
    /// <remarks>
    /// This projection is intended for use in LINQ queries executed by a query provider
    /// such as Entity Framework Core. Because it is an expression tree, it can be
    /// translated into SQL, allowing only the required fields to be selected from
    /// the database.
    ///
    /// The projection includes:
    /// <list type="bullet">
    /// <item><description>Basic identity and profile information</description></item>
    /// <item><description>Password policy-related fields</description></item>
    /// <item><description>Activation state</description></item>
    /// <item><description>User permissions mapped into <see cref="Permission"/> value objects</description></item>
    /// <item><description>Partition access identifiers</description></item>
    /// </list>
    ///
    /// <para>
    /// Nested collections are projected using LINQ <c>Select</c>. Depending on the provider,
    /// this may result in SQL joins or partial client-side evaluation.
    /// </para>
    /// </remarks>
    public static readonly Expression<Func<User, UserInformation>> InformationProjection =
        u => new UserInformation(
            u.Guid,
            u.Nameid,
            u.FirstName,
            u.LastName,
            u.Description,
            u.DefaultPasswordExpirationPeriod,
            u.RequirePasswordChangeAt,
            u.IsActive,
            u.Permissions.Select(p => new Permission(p.Guid, p.Action)).ToList(),
            u.PartitionAccesses.Select(p => p.Guid).ToList(),
            u.EditedByGuid
        );

    /// <summary>
    /// Maps a <see cref="User"/> entity to a <see cref="UserInformation"/> object.
    /// </summary>
    /// <param name="u">The <see cref="User"/> to map.</param>
    /// <returns>
    /// A new <see cref="UserInformation"/> instance containing the mapped data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="u"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method performs an in-memory transformation and should be used only when
    /// the <see cref="User"/> entity has already been loaded into memory.
    ///
    /// The permissions collection is fully enumerated and mapped into
    /// <see cref="Permission"/> value objects.
    ///
    /// The partition access collection is projected as a set of partition identifiers,
    /// representing the partitions the user has direct access to.
    ///
    /// For queryable scenarios (e.g., database queries), prefer using
    /// <see cref="InformationProjection"/> to ensure efficient execution.
    /// </remarks>
    public static UserInformation ToInformation(this User u)
    {
        ArgumentNullException.ThrowIfNull(u);

        return new UserInformation(
            u.Guid,
            u.Nameid,
            u.FirstName,
            u.LastName,
            u.Description,
            u.DefaultPasswordExpirationPeriod,
            u.RequirePasswordChangeAt,
            u.IsActive,
            [.. u.Permissions.Select(p => new Permission(p.Guid, p.Action))],
            [.. u.PartitionAccesses.Select(p => p.Guid)],
            u.EditedByGuid
        );
    }
}
