namespace Fargo.Application.Dtos.ItemDtos
{
    public record ItemUpdateDto(
        OptionalSetDto<Guid>? ParentItemGuid);
}
