using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core;
using NSubstitute;

namespace Fargo.Application.Tests;

public sealed class ApplicationServiceBoundaryTests
{
    [Fact]
    public async Task ItemUpdate_Should_DispatchFocusedCommands_And_SaveOnce()
    {
        var setParent = Substitute.For<ICommandHandler<ItemSetParentContainerCommand>>();
        var setPartitions = Substitute.For<ICommandHandler<ItemSetPartitionsCommand>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var sut = new ItemApplicationService(
            Substitute.For<ICommandHandler<ItemCreateCommand, Guid>>(),
            setParent,
            setPartitions,
            Substitute.For<ICommandHandler<ItemDeleteCommand>>(),
            unitOfWork);
        var itemGuid = Guid.NewGuid();
        var parentGuid = Guid.NewGuid();
        IReadOnlyCollection<Guid> partitions = [Guid.NewGuid()];

        await sut.Update(itemGuid, new ItemUpdateDto(partitions, parentGuid));

        await setParent.Received(1).Handle(
            Arg.Is<ItemSetParentContainerCommand>(command =>
                command.ItemGuid == itemGuid &&
                command.ParentContainerGuid == parentGuid),
            Arg.Any<CancellationToken>());
        await setPartitions.Received(1).Handle(
            Arg.Is<ItemSetPartitionsCommand>(command =>
                command.ItemGuid == itemGuid &&
                command.PartitionGuids == partitions),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PartitionCreate_Should_DispatchCreateParent_And_SaveOnce()
    {
        var create = Substitute.For<ICommandHandler<PartitionCreateCommand, Guid>>();
        var setParent = Substitute.For<ICommandHandler<PartitionSetParentCommand>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var partitionGuid = Guid.NewGuid();
        var parentGuid = Guid.NewGuid();
        create.Handle(Arg.Any<PartitionCreateCommand>(), Arg.Any<CancellationToken>())
            .Returns(partitionGuid);
        var sut = new PartitionApplicationService(
            create,
            Substitute.For<ICommandHandler<PartitionRenameCommand>>(),
            Substitute.For<ICommandHandler<PartitionChangeDescriptionCommand>>(),
            setParent,
            Substitute.For<ICommandHandler<PartitionActivateCommand>>(),
            Substitute.For<ICommandHandler<PartitionDeactivateCommand>>(),
            Substitute.For<ICommandHandler<PartitionDeleteCommand>>(),
            unitOfWork);

        var result = await sut.Create(new PartitionCreateDto(new Name("Partition"), ParentPartitionGuid: parentGuid));

        Assert.Equal(partitionGuid, result);
        await setParent.Received(1).Handle(
            Arg.Is<PartitionSetParentCommand>(command =>
                command.PartitionGuid == partitionGuid &&
                command.ParentPartitionGuid == parentGuid),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserGroupUpdate_Should_DispatchProvidedFields_And_SaveOnce()
    {
        var changeDescription = Substitute.For<ICommandHandler<UserGroupChangeDescriptionCommand>>();
        var activate = Substitute.For<ICommandHandler<UserGroupActivateCommand>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var userGroupGuid = Guid.NewGuid();
        var description = new Description("Updated group");
        var sut = new UserGroupApplicationService(
            Substitute.For<ICommandHandler<UserGroupCreateCommand, Guid>>(),
            Substitute.For<ICommandHandler<UserGroupChangeNameidCommand>>(),
            changeDescription,
            Substitute.For<ICommandHandler<UserGroupSetPermissionsCommand>>(),
            Substitute.For<ICommandHandler<UserGroupSetPartitionsCommand>>(),
            activate,
            Substitute.For<ICommandHandler<UserGroupDeactivateCommand>>(),
            Substitute.For<ICommandHandler<UserGroupDeleteCommand>>(),
            unitOfWork);

        await sut.Update(userGroupGuid, new UserGroupUpdateDto(null, description, true, null, null));

        await changeDescription.Received(1).Handle(
            Arg.Is<UserGroupChangeDescriptionCommand>(command =>
                command.UserGroupGuid == userGroupGuid &&
                command.Description == description),
            Arg.Any<CancellationToken>());
        await activate.Received(1).Handle(
            Arg.Is<UserGroupActivateCommand>(command => command.UserGroupGuid == userGroupGuid),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserCreate_Should_DispatchCreate_And_SaveOnce()
    {
        var create = Substitute.For<ICommandHandler<UserCreateCommand, Guid>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var userGuid = Guid.NewGuid();
        create.Handle(Arg.Any<UserCreateCommand>(), Arg.Any<CancellationToken>())
            .Returns(userGuid);
        var sut = new UserApplicationService(
            create,
            Substitute.For<ICommandHandler<UserChangeNameidCommand>>(),
            Substitute.For<ICommandHandler<UserChangeFirstNameCommand>>(),
            Substitute.For<ICommandHandler<UserChangeLastNameCommand>>(),
            Substitute.For<ICommandHandler<UserChangeDescriptionCommand>>(),
            Substitute.For<ICommandHandler<UserSetDefaultPasswordExpirationCommand>>(),
            Substitute.For<ICommandHandler<UserChangePasswordCommand>>(),
            Substitute.For<ICommandHandler<UserSetPermissionsCommand>>(),
            Substitute.For<ICommandHandler<UserSetPartitionsCommand>>(),
            Substitute.For<ICommandHandler<UserSetUserGroupsCommand>>(),
            Substitute.For<ICommandHandler<UserActivateCommand>>(),
            Substitute.For<ICommandHandler<UserDeactivateCommand>>(),
            Substitute.For<ICommandHandler<UserDeleteCommand>>(),
            unitOfWork);

        var result = await sut.Create(new UserCreateDto("valid-user", "ValidPass1!"));

        Assert.Equal(userGuid, result);
        await create.Received(1).Handle(
            Arg.Is<UserCreateCommand>(command =>
                command.Nameid == new Nameid("valid-user") &&
                command.Password == new Password("ValidPass1!")),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }
}
