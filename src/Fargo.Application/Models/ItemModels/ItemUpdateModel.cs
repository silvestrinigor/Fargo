namespace Fargo.Application.Models.ItemModels
{
    public record ItemUpdateModel(
        OptionalSetModel<Guid>? ParentItemGuid);
}
