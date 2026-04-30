using Fargo.Application.Authentication;
using Fargo.Domain.Events;
using Fargo.Infrastructure.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Events;

public sealed class BufferedEventPublisherTests
{
    [Fact]
    public async Task Record_Should_AddEventToCurrentUnitOfWork()
    {
        var currentUser = new StubCurrentUser(Guid.NewGuid());
        var db = new FargoDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var recorder = new DbEventRecorder(db, currentUser);
        var articleGuid = Guid.NewGuid();

        await recorder.Record(EventType.ArticleUpdated, EntityType.Article, articleGuid);

        var localEvent = Assert.Single(db.Events.Local);

        Assert.Equal(EventType.ArticleUpdated, localEvent.EventType);
        Assert.Equal(EntityType.Article, localEvent.EntityType);
        Assert.Equal(articleGuid, localEvent.EntityGuid);
        Assert.Equal(currentUser.UserGuid, localEvent.ActorGuid);
    }

    private sealed class StubCurrentUser(Guid userGuid) : ICurrentUser
    {
        public Guid UserGuid => userGuid;

        public bool IsAuthenticated => true;
    }
}
