using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class ItemContainer(ContainerProperties information)
    {
        public ContainerProperties Information { get; } = information;

        public int Count => items.Count;

        public Mass? AvailableMass { get; private set; } = information.MaxMass;

        public Volume? AvailableVolume { get; private set; } = information.VolumeSpace;

        private readonly List<Item> items = [];

        public bool Contains(Item item) => items.Contains(item);

        public void Add(Item item)
        {
            AvailableMass -= item.Article.Properties?.Mass;
            AvailableVolume -= item.Article.Properties?.Volume;
            items.Add(item);
        }

        public bool Remove(Item item)
        {
            AvailableMass += item.Article.Properties?.Mass;
            AvailableVolume += item.Article.Properties?.Volume;
            return items.Remove(item);
        }
    }
}
