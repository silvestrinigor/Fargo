using UnitsNet.NumberExtensions.NumberToDensity;
using UnitsNet.NumberExtensions.NumberToLength;
using UnitsNet.NumberExtensions.NumberToMass;
using UnitsNet.NumberExtensions.NumberToVolume;

namespace Fargo.Domain.Tests.EntitiesTests.ArticleTests
{
    public class ArticleTests
    {
        [Fact]
        public void CreateArticle_WithLengthWidthHeight_ShouldCalculateVolume()
        {
            var article = new Article
            {
                Length = 2.Meters(),
                Width = 3.Meters(),
                Height = 4.Meters()
            };
            var expectedVolume = 24.CubicMeters();

            Assert.Equal(expectedVolume, article.Volume);
        }

        [Fact]
        public void CreateArticle_WithMassAndVolume_ShouldCalculateDensity()
        {
            var article = new Article
            {
                Mass = 12.Kilograms(),
                Length = 2.Meters(),
                Width = 2.Meters(),
                Height = 3.Meters()
            };
            var expectedDensity = 1.KilogramsPerCubicMeter();

            Assert.Equal(expectedDensity, article.Density);
        }
    }
}
