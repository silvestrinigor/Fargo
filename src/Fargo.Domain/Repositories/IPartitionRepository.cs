using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="Partition"/> entities.
    ///
    /// This repository provides access to partition persistence operations and
    /// domain-related queries involving partitions.
    /// </summary>
    public interface IPartitionRepository
    {
        /// <summary>
        /// Gets a partition by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the partition.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="Partition"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<Partition?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Adds a new partition to the persistence context.
        /// </summary>
        /// <param name="partition">The partition to add.</param>
        void Add(Partition partition);

        /// <summary>
        /// Removes a partition from the persistence context.
        /// </summary>
        /// <param name="partition">The partition to remove.</param>
        void Remove(Partition partition);
    }
}