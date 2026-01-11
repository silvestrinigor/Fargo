namespace Fargo.Application.Models.PartitionModels
{
    public sealed record PartitionReadModel(
        Guid Guid,
        string Name,
        string Description
        ) : IEntityByGuidReadModel, IEntityTemporalReadModel;
}
