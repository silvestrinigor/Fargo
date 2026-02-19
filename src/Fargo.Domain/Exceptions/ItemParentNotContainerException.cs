using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class ItemParentNotContainerException(Item parentItem) 
        : FargoException
    {
        public Item ParentItem 
        { 
            get; 
        } = parentItem;
    }
}