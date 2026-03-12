namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Defines a contract for entities whose visibility is scoped by partitions.
    /// </summary>
    public interface IPartitioned
    {
        /// <summary>
        /// Gets the partitions associated with the current entity.
        /// </summary>
        IReadOnlyCollection<Partition> Partitions { get; }
    }
}