using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Domain.Entities;

namespace Fargo.Application.Extensions
{
    public static class PartitionExtension
    {
        extension(Partition partition)
        {
            public PartitionDto ToDto()
                => new(
                    partition.Guid,
                    partition.Name,
                    partition.Description
                    );
        }
    }
}
