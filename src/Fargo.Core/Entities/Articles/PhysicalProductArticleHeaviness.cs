using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class PhysicalProductArticleHeaviness
    {
        public Mass? Mass { get; }

        public Density? Density { get; }

        public PhysicalProductArticleHeaviness() { }

        public PhysicalProductArticleHeaviness(Mass mass)
        {
            Mass = mass;
        }

        public PhysicalProductArticleHeaviness(Mass mass, Volume volume)
        {
            Mass = mass;
            Density = mass / volume;
        }
    }
}
