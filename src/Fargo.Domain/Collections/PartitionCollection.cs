using Fargo.Domain.Entities;
using System.Collections.ObjectModel;

namespace Fargo.Domain.Collections;

/// <summary>
/// Represents a collection of <see cref="Partition"/> instances associated with an entity.
/// </summary>
/// <remarks>
/// This collection enforces domain rules for entity partitions, such as preventing
/// duplicate items and rejecting <see langword="null"/> values.
/// </remarks>
public sealed class PartitionCollection : Collection<Partition>
{
    /// <summary>
    /// Initializes an empty <see cref="PartitionCollection"/>.
    /// </summary>
    public PartitionCollection()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="PartitionCollection"/> with the specified partitions.
    /// </summary>
    /// <param name="partitions">
    /// The partitions to populate the collection with.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partitions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains duplicate partitions.
    /// </exception>
    public PartitionCollection(IEnumerable<Partition> partitions)
    {
        ArgumentNullException.ThrowIfNull(partitions);

        foreach (var partition in partitions)
        {
            Add(partition);
        }
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, Partition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item))
        {
            throw new InvalidOperationException(
                "The partition already exists in the collection.");
        }

        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    protected override void SetItem(int index, Partition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item) && !ReferenceEquals(Items[index], item))
        {
            throw new InvalidOperationException(
                "The partition already exists in the collection.");
        }

        base.SetItem(index, item);
    }

    /// <summary>
    /// Determines whether the specified partition exists in the collection.
    /// </summary>
    /// <param name="partition">
    /// The partition to locate in the collection.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified partition exists in the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public bool HasPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);
        return Items.Contains(partition);
    }
}
