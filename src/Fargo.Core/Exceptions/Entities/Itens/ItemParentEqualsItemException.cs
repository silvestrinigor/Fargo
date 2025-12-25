namespace Fargo.Domain.Exceptions.Entities.Itens
{
    public class ItemParentEqualsItemException()
        : FargoException("Parent item cannot be equals the child item.");
}
