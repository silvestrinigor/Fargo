using System.Linq.Expressions;
using Fargo.Application.Models.TreeModels;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Application.Mappings
{
    public static class PartitionMappings
    {
        public static readonly Expression<Func<Partition, PartitionInformation>> InformationProjection =
            p => new PartitionInformation(
                p.Guid,
                p.Name,
                p.Description,
                p.ParentPartitionGuid,
                p.IsActive
            );

        public static PartitionInformation ToInformation(this Partition p) =>
            new(
                p.Guid,
                p.Name,
                p.Description,
                p.ParentPartitionGuid,
                p.IsActive
            );
    }
}