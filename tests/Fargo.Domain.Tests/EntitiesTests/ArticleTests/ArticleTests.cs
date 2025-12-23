using UnitsNet.NumberExtensions.NumberToDensity;
using UnitsNet.NumberExtensions.NumberToLength;
using UnitsNet.NumberExtensions.NumberToMass;
using UnitsNet.NumberExtensions.NumberToTemperature;
using UnitsNet.NumberExtensions.NumberToVolume;

namespace Fargo.Domain.Tests.EntitiesTests.ArticleTests
{
    public class ArticleTests
    {
        [Fact]
        public void CreateArticle_WithNegativeMass_ShouldThrowException()
        {
            static void action()
            {
                _ = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
                {
                    Mass = -5.Kilograms()
                };
            }

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void CreateArticle_WithNegativeLength_ShouldThrowException()
        {
            static void action()
            {
                _ = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
                {
                    Length = -10.Meters()
                };
            }

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void CreateArticle_WithNegativeWidth_ShouldThrowException()
        {
            static void action()
            {
                _ = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
                {
                    Width = -3.Meters()
                };
            }

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void CreateArticle_WithNegativeHeight_ShouldThrowException()
        {
            static void action()
            {
                _ = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
                {
                    Height = -7.Meters()
                };
            }

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void CreateArticle_WithInvalidTemperatureRange_ShouldThrowException()
        {
            static void action()
            {
                _ = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
                {
                    TemperatureMin = 10.DegreesCelsius(),
                    TemperatureMax = 5.DegreesCelsius()
                };
            }

            Assert.Throws<ArgumentException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void CreateArticle_WithLengthWidthHeight_ShouldCalculateVolume()
        {
            var article = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
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
            var article = new Fargo.Domain.Entities.Articles.PhysicalProductArticle
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
