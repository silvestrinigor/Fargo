namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionResponseModel(
            Guid Guid,
            string Name,
            string Description
            );
}