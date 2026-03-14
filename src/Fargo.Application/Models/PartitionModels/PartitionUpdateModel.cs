using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.PartitionModels
{
    /// <summary>
    /// Represents the data used to update an existing partition.
    /// </summary>
    /// <remarks>
    /// This model contains the mutable properties of a partition that can be
    /// modified through update operations.
    ///
    /// Any property left as <see langword="null"/> indicates that the value
    /// should not be changed during the update process.
    /// </remarks>
    /// <param name="Description">
    /// The new description for the partition.
    /// If <see langword="null"/>, the description will not be modified.
    /// </param>
    public sealed record PartitionUpdateModel(
            Description? Description = null
            );
}