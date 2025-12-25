namespace Fargo.Domain.Exceptions.Entities.Itens
{
    public class InsufficientAvailableCapacityException()
        : FargoException("The available capacity is not sufficient.");
}
