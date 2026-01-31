using Fargo.Domain.Entities;

namespace Fargo.Domain.Exceptions
{
    public class ItemParentInsideItemException(Item item, Item parentItem)
        : FargoException
    {
        public Item Item
        {
            get;
        } = item;

        public Item ParentItem 
        {
            get;
        } = parentItem;
    }
}