using Fargo.Domain.Abstracts.Entities;
using UnitsNet;

namespace Fargo.Domain.Entities
{
    public class Article : NamedEntity
    {
        public Mass? ItemMass { get; private set; }
        public Length? ItemLength { get; private set; }
        public Length? ItemWidth { get; private set; }
        public Length? ItemHeight { get; private set; }
        public Density? Density => ItemMass is not null && ItemVolume is not null 
            ? ItemMass / ItemVolume
            : null;

        public Volume? ItemVolume => ItemLength is not null && ItemWidth is not null && ItemHeight is not null
            ? ItemLength * ItemWidth * ItemHeight 
            : null;
    }
}