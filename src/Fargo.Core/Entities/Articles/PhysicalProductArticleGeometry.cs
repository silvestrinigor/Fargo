using UnitsNet;

namespace Fargo.Domain.Entities.Articles
{
    public class PhysicalProductArticleGeometry
    {
        public Length? X { get; }

        public Length? Y { get; }

        public Length? Z { get; }

        public Volume? Volume { get; }

        public PhysicalProductArticleGeometry() { }

        public PhysicalProductArticleGeometry(Volume v)
        {
            Volume = v;
        }

        public PhysicalProductArticleGeometry(Length x, Length y, Length z)
        {
            X = x;
            Y = y;
            Z = z;

            Volume = x * y * z;
        }
    }
}
