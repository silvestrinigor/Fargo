using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.Items;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemUpdateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            partitionRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenAddingPartition_ShouldAddPartitionToItem()
    {
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditItem], [FargoCoreWellKnowGuids.GlobalPartitionGuid, partition.Guid]);

        var command = new ItemUpdateCommand(item.Guid, new ItemUpdateDto(PartitionsToAdd: [partition.Guid]));

        await CreateHandler().HandleAsync(command);

        Assert.Contains(partition.Guid, item.PartitionGuids);
    }
    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(Guid.NewGuid(), new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(Guid.NewGuid(), new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(missing, new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditItem], partitionAccesses: [Guid.NewGuid()]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(item.Guid, new ItemUpdateDto())));
    }
}
