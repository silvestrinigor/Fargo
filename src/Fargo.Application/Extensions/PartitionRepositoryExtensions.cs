using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IPartitionRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
/// <remarks>
/// These helpers encapsulate common patterns such as retrieving entities
/// and ensuring their existence, promoting consistency and reducing
/// repetitive null-check logic across the application layer.
/// </remarks>
public static class PartitionRepositoryExtensions
{
    extension(IPartitionRepository repository)
    {
        /// <summary>
        /// Retrieves a <see cref="Partition"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="partitionGuid">
        /// The unique identifier of the partition.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="Partition"/> associated with the specified GUID.
        /// </returns>
        /// <exception cref="PartitionNotFoundFargoApplicationException">
        /// Thrown when no partition is found with the specified GUID.
        /// </exception>
        /// <remarks>
        /// This method follows a fail-fast approach by throwing an exception
        /// when the requested entity does not exist, eliminating the need
        /// for null checks in the calling code.
        /// </remarks>
        public async Task<Partition> GetFoundByGuid(
            Guid partitionGuid,
            CancellationToken cancellationToken = default
        )
        {
            var partition = await repository.GetByGuid(partitionGuid, cancellationToken)
                ?? throw new PartitionNotFoundFargoApplicationException(partitionGuid);

            return partition;
        }
    }
}
