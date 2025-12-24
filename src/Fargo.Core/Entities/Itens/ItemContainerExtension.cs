using UnitsNet;

namespace Fargo.Domain.Entities.Itens
{
    public class ItemContainerExtension(Item item)
    {
        public Mass? MassAvailableCapacity { get; private set; } = item.Article.Container?.MassCapacity;

        public Volume? VolumeAvailableCapacity { get; private set; } = item.Article.Container?.VolumeCapacity;

        public Item ValidateAddition(Item item) =>
            VolumeAvailableCapacity < item.Article.Volume
            ? throw new InvalidOperationException()
            :
            MassAvailableCapacity < item.Article.Mass
            ? throw new InvalidOperationException()
            : item;

        public void Add(Item item)
        {
            MassAvailableCapacity -= item.Article.Mass;
            VolumeAvailableCapacity -= item.Article.Volume;
        }

        public void Remove(Item item)
        {
            MassAvailableCapacity += item.Article.Mass;
            VolumeAvailableCapacity += item.Article.Volume;
        }
    }
}
