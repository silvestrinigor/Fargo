using Fargo.Domain.Partitions;
using System.Linq.Expressions;

namespace Fargo.Application.Mappings;

/// <summary>
/// Provides mapping utilities for transforming <see cref="Partition"/> entities
/// into lightweight data transfer representations.
/// </summary>
/// <remarks>
/// This class centralizes projection logic for <see cref="Partition"/> to ensure
/// consistency across queries and in-memory transformations.
///
/// Two mapping approaches are provided:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="InformationProjection"/>: An expression-based projection that can be
/// translated by LINQ providers (e.g., Entity Framework Core) into SQL queries,
/// allowing efficient data retrieval without loading full entities.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="ToInformation(Partition)"/>: An in-memory mapping method used when
/// the entity has already been materialized.
/// </description>
/// </item>
/// </list>
/// </remarks>
public static class PartitionMappings
{
    /// <summary>
    /// Gets an expression used to project a <see cref="Partition"/> entity
    /// into a <see cref="PartitionInformation"/> object.
    /// </summary>
    /// <remarks>
    /// This projection is intended for use in LINQ queries executed by a query provider
    /// such as Entity Framework Core. Because it is an expression tree, it can be
    /// translated into SQL, allowing only the required fields to be selected from
    /// the database.
    ///
    /// Using this projection avoids materializing the full <see cref="Partition"/> entity,
    /// improving performance for read operations.
    ///
    /// The projected data includes structural information about the partition,
    /// such as its parent relationship and activation state, but excludes
    /// navigation properties and access-related collections.
    /// </remarks>
    public static readonly Expression<Func<Partition, PartitionInformation>> InformationProjection =
        p => new PartitionInformation(
            p.Guid,
            p.Name,
            p.Description,
            p.ParentPartitionGuid,
            p.IsActive
        );

    /// <summary>
    /// Maps a <see cref="Partition"/> entity to a <see cref="PartitionInformation"/> object.
    /// </summary>
    /// <param name="p">The <see cref="Partition"/> to map.</param>
    /// <returns>
    /// A new <see cref="PartitionInformation"/> instance containing the mapped data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="p"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method performs an in-memory transformation and should be used only when
    /// the <see cref="Partition"/> entity has already been loaded into memory.
    ///
    /// For queryable scenarios (e.g., database queries), prefer using
    /// <see cref="InformationProjection"/> to ensure efficient execution.
    /// </remarks>
    public static PartitionInformation ToInformation(this Partition p)
    {
        ArgumentNullException.ThrowIfNull(p);

        return new PartitionInformation(
            p.Guid,
            p.Name,
            p.Description,
            p.ParentPartitionGuid,
            p.IsActive
        );
    }
}
