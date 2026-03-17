using Fargo.Domain.Entities;
using System.Collections.ObjectModel;

namespace Fargo.Domain.Collections;

/// <summary>
/// Represents a collection of <see cref="UserGroup"/> instances.
/// </summary>
/// <remarks>
/// This collection enforces domain rules for user groups, such as preventing
/// duplicate items and rejecting <see langword="null"/> values.
/// </remarks>
public sealed class UserGroupCollection : Collection<UserGroup>
{
    /// <summary>
    /// Initializes an empty <see cref="UserGroupCollection"/>.
    /// </summary>
    public UserGroupCollection()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="UserGroupCollection"/> with the specified groups.
    /// </summary>
    /// <param name="groups">
    /// The groups used to populate the collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="groups"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when duplicate groups are found.
    /// </exception>
    public UserGroupCollection(IEnumerable<UserGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        foreach (var group in groups)
        {
            Add(group);
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group already exists in the collection.
    /// </exception>
    protected override void InsertItem(int index, UserGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item))
        {
            throw new InvalidOperationException(
                "The user group already exists in the collection.");
        }

        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group already exists in the collection.
    /// </exception>
    protected override void SetItem(int index, UserGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item) && !ReferenceEquals(Items[index], item))
        {
            throw new InvalidOperationException(
                "The user group already exists in the collection.");
        }

        base.SetItem(index, item);
    }

    /// <summary>
    /// Determines whether the specified group exists in the collection.
    /// </summary>
    /// <param name="group">
    /// The group to locate in the collection.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified group exists in the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="group"/> is <see langword="null"/>.
    /// </exception>
    public bool HasGroup(UserGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return Items.Contains(group);
    }
}
