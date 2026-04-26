using Fargo.Domain.Items;
using System.Linq.Expressions;

namespace Fargo.Application.Items;

/// <summary>
/// Provides mapping utilities for transforming <see cref="Item"/> entities
/// into lightweight data transfer representations.
/// </summary>
/// <remarks>
/// This class centralizes projection logic for <see cref="Item"/> to ensure
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
/// <see cref="ToInformation(Item)"/>: An in-memory mapping method used when
/// the entity has already been materialized.
/// </description>
/// </item>
/// </list>
/// </remarks>
public static class ItemMappings
{
    /// <summary>
    /// Gets an expression used to project an <see cref="Item"/> entity
    /// into an <see cref="ItemInformation"/> object.
    /// </summary>
    /// <remarks>
    /// This projection is intended for use in LINQ queries executed by a query provider
    /// such as Entity Framework Core. Because it is an expression tree, it can be
    /// translated into SQL, allowing only the required fields to be selected from
    /// the database.
    ///
    /// Using this projection avoids materializing the full <see cref="Item"/> entity,
    /// improving performance for read operations.
    /// </remarks>
    public static readonly Expression<Func<Item, ItemInformation>> InformationProjection =
        i => new ItemInformation(
            i.Guid,
            i.ArticleGuid,
            i.ProductionDate,
            i.EditedByGuid
        );

    /// <summary>
    /// Maps an <see cref="Item"/> entity to an <see cref="ItemInformation"/> object.
    /// </summary>
    /// <param name="i">The <see cref="Item"/> to map.</param>
    /// <returns>
    /// A new <see cref="ItemInformation"/> instance containing the mapped data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="i"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method performs an in-memory transformation and should be used only when
    /// the <see cref="Item"/> entity has already been loaded into memory.
    ///
    /// For queryable scenarios (e.g., database queries), prefer using
    /// <see cref="InformationProjection"/> to ensure efficient execution.
    /// </remarks>
    public static ItemInformation ToInformation(this Item i)
    {
        ArgumentNullException.ThrowIfNull(i);

        return new ItemInformation(
            i.Guid,
            i.ArticleGuid,
            i.ProductionDate,
            i.EditedByGuid
        );
    }
}
