namespace Fargo.Application.Exceptions
{
    public class EntityNotFoundException() 
        : FargoApplicationException($"Entity not found.");
}
