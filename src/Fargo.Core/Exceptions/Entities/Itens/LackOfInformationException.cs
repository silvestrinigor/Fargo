namespace Fargo.Domain.Exceptions.Entities.Itens
{
    public class LackOfInformationException()
        : FargoException("Item does not have required information for this operation.");
}
