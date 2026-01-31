using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class ItemParentEqualsItemException(Item item) 
        : FargoException
    {
        public Item Item
        {
            get;
        } = item;
    }
}