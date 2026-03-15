using Fargo.Domain.Entities;
using Fargo.Domain.Logics;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services
{
    /// <summary>
    /// Provides domain operations related to <see cref="Partition"/> access.
    /// </summary>
    /// <remarks>
    /// This service encapsulates logic for retrieving partitions while enforcing
    /// access rules based on the permissions of the acting <see cref="User"/>.
    /// </remarks>
    public class PartitionService(
            IPartitionRepository partitionRepository)
    {
        /// <summary>
        /// Gets a partition by its identifier if the specified actor
        /// has access to it.
        /// </summary>
        /// <param name="partitionGuid">
        /// The unique identifier of the partition to retrieve.
        /// </param>
        /// <param name="actor">
        /// The user performing the operation. The actor must have
        /// access to the requested partition.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// The matching <see cref="Partition"/> when it exists and the actor
        /// has access to it; otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// This method ensures that a partition is only returned when the
        /// requesting user has access to it according to the partition
        /// access rules defined in the domain.
        /// </remarks>
        public async Task<Partition?> GetPartition(
                Guid partitionGuid,
                User actor,
                CancellationToken cancellationToken = default)
        {
            var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

            if (partition != null && !partition.HasAccess(actor))
            {
                return null;
            }

            return partition;
        }

        public static bool HasAccess(Partition partition, User user)
        {
            var userHasAccess = partition.HasAccess(user);

            if (userHasAccess)
            {
                return true;
            }

            var userGroupHasAccess = user.UserGroups.Any(partition.HasAccess);

            return userGroupHasAccess;
        }

        public static bool HasAccess(IPartitioned partitioned, User user)
        {
            var userHasAccess = partitioned.HasAccess(user);

            if (userHasAccess)
            {
                return true;
            }

            var userGroupHasAccess = user.UserGroups.Any(partitioned.HasAccess);

            return userGroupHasAccess;
        }
    }
}