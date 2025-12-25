namespace Fargo.Domain.Exceptions.Entities.Itens
{
    public class ItemAlreadyInsideContainerException()
        : FargoException("Item is already inside the container.");
}
