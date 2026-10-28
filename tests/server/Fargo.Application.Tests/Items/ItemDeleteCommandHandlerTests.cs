using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemDeleteCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            auditLogRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveItemAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteItem]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        await CreateHandler().HandleAsync(new ItemDeleteCommand(item.Guid));

        itemRepository.Received(1).Remove(item);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(missing)));
    }
}
