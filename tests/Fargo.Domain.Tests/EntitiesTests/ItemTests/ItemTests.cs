using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Entities.Itens;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToLength;
using UnitsNet.NumberExtensions.NumberToMass;
using UnitsNet.NumberExtensions.NumberToTemperature;
using UnitsNet.NumberExtensions.NumberToVolume;

namespace Fargo.Domain.Tests.EntitiesTests.ItemTests
{
    public class ItemTests
    {
        [Fact]
        public void Add_WhenItemIsNotContainer_ShouldThrowException()
        {
            var article = new PhisicalItemArticle
            {
                IsContainer = false
            };
            var item = new Item
            {
                Article = article
            };
            var child = new Item
            {
                Article = new PhisicalItemArticle()
            };

            void action() => item.Add(child);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(item, child.Container);
        }

        [Fact]
        public void Add_WhenItemMassCapacityExceeded_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
                ContainerMassCapacity = 10.Kilograms()
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var heavyArticle = new PhisicalItemArticle
            {
                Mass = 11.Kilograms()
            };
            var heavyItem = new Item
            {
                Article = heavyArticle
            };

            void action() => containerItem.Add(heavyItem);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(containerItem, heavyItem.Container);
        }

        [Fact]
        public void Add_WhenItemVolumeCapacityExceeded_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
                ContainerVolumeCapacity = 5.CubicMeters()
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var largeArticle = new PhisicalItemArticle
            {
                Length = 3.Meters(),
                Width = 2.Meters(),
                Height = 1.Meters(),
            };
            var largeItem = new Item
            {
                Article = largeArticle
            };

            void action() => containerItem.Add(largeItem);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(containerItem, largeItem.Container);
        }

        [Fact]
        public void Add_WhenItemMaxTemperatureExceeded_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                TemperatureMax = 50.DegreesCelsius(),
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var hotArticle = new PhisicalItemArticle
            {
                TemperatureMax = 49.DegreesCelsius()
            };
            var hotItem = new Item
            {
                Article = hotArticle
            };

            void action() => containerItem.Add(hotItem);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(containerItem, hotItem.Container);
        }

        [Fact]
        public void Add_WhenItemMinTemperatureExceeded_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                TemperatureMin = 0.DegreesCelsius(),
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var coldArticle = new PhisicalItemArticle
            {
                TemperatureMin = 1.DegreesCelsius()
            };
            var coldItem = new Item
            {
                Article = coldArticle
            };
            void action() => containerItem.Add(coldItem);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(containerItem, coldItem.Container);
        }

        [Fact]
        public void Add_WhenItemIsItself_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };

            void action() => containerItem.Add(containerItem);
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(containerItem.Container, containerItem);
        }

        [Fact]
        public void Add_WhenItemContainerIsNotContainerContainer_ShouldThrowException()
        {
            var grandContainerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var grandContainerItem = new Item
            {
                Article = grandContainerArticle
            };
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            grandContainerItem.Add(containerItem);
            var itemArticle = new PhisicalItemArticle { };
            var item = new Item
            {
                Article = itemArticle
            };
            
            void action() => containerItem.Add(item);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
            Assert.NotEqual(grandContainerItem, item.Container);
        }

        [Fact]
        public void Add_WhenAllConditionsMet_ShouldAddItemSuccessfully()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var itemArticle = new PhisicalItemArticle
            {
                Length = 3.Meters(),
                Width = 2.Meters(),
                Height = 1.Meters(),
                Mass = 10.Kilograms()
            };
            var item = new Item
            {
                Article = itemArticle
            };

            containerItem.Add(item);

            Assert.Equal(containerItem, item.Container);
            Assert.Equal(10.Kilograms(), containerItem.ContainedMass);
            Assert.Equal(6.CubicMeters(), containerItem.ContainedVolume);
        }

        [Fact]
        public void Remove_WhenItemNotInContainer_ShouldThrowException()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var itemArticle = new PhisicalItemArticle
            {
                Length = 3.Meters(),
                Width = 2.Meters(),
                Height = 1.Meters(),
                Mass = 10.Kilograms()
            };
            var item = new Item
            {
                Article = itemArticle
            };

            void action() => containerItem.Remove(item);

            Assert.Throws<InvalidOperationException>(() =>
            {
                action();
            });
        }

        [Fact]
        public void Remove_WhenItemInContainer_ShouldRemoveItemSuccessfully()
        {
            var containerArticle = new PhisicalItemArticle
            {
                IsContainer = true,
            };
            var containerItem = new Item
            {
                Article = containerArticle
            };
            var itemArticle = new PhisicalItemArticle { };
            var item = new Item
            {
                Article = itemArticle
            };
            containerItem.Add(item);

            containerItem.Remove(item);

            Assert.Equal(item.Container, containerItem.Container);
            Assert.Equal(Mass.Zero, containerItem.ContainedMass);
            Assert.Equal(Volume.Zero, containerItem.ContainedVolume);
        }
    }
}
