namespace Fargo.Application.Models.ItemModels
{
    public record ItemUpdateModel(
        OptionalSet<Guid?>? ParentItemGuid = null);
}
