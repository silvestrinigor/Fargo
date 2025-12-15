using Fargo.Domain.Entities.Articles;

namespace Fargo.Domain.Tests.EntitiesTests
{
    public class EntityTests
    {
        [Fact]
        public void CreateEntity_ShouldHaveGuidAssigned()
        {
            var entity = new Article();
            Assert.NotEqual(Guid.Empty, entity.Guid);
        }

        [Fact]
        public void Equals_SameGuid_ShouldReturnTrue()
        {
            var guid = Guid.NewGuid();
            var entity1 = new Article { Guid = guid };
            var entity2 = new Article { Guid = guid };

            Assert.True(entity1.Equals(entity2));
            Assert.True(entity1 == entity2);
        }

        [Fact]
        public void CreateEntity_ShouldHaveCreatedAtAssigned()
        {
            var beforeCreation = DateTime.UtcNow;
            var entity = new Article();
            var afterCreation = DateTime.UtcNow;

            Assert.InRange(entity.CreatedAt, beforeCreation, afterCreation);
        }
    }
}
