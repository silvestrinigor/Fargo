using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.System;
using Fargo.Domain.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Articles;

public sealed class ArticleQueryActorAccessTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IArticleQueryRepository articleRepository = Substitute.For<IArticleQueryRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task ManyQuery_Should_PassSystemEffectivePartitionsToRepository()
    {
        var partitionGuids = new[] { PartitionService.GlobalPartitionGuid, Guid.NewGuid() };
        currentUser.UserGuid.Returns(SystemService.SystemGuid);
        partitionRepository
            .GetDescendantGuids(PartitionService.GlobalPartitionGuid, true, Arg.Any<CancellationToken>())
            .Returns(partitionGuids);
        var sut = new ArticlesQueryHandler(
            new ActorService(userRepository, partitionRepository),
            articleRepository,
            currentUser);

        await sut.Handle(new ArticlesQuery(new Pagination(Page.FirstPage, Limit.MaxLimit)));

        await articleRepository.Received(1).GetManyInfo(
            Arg.Any<Pagination>(),
            null,
            Arg.Is<IReadOnlyCollection<Guid>>(guids => guids.SequenceEqual(partitionGuids)),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManyQuery_Should_IntersectRequestedPartitionsWithAdminDatabaseEffectivePartitions()
    {
        var globalPartition = CreatePartition(PartitionService.GlobalPartitionGuid, "Global");
        var accessibleChildGuid = Guid.NewGuid();
        var inaccessiblePartitionGuid = Guid.NewGuid();
        var admin = new User
        {
            Guid = UserService.DefaultAdministratorUserGuid,
            Nameid = new Nameid("admin"),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
        admin.AddPartitionAccess(globalPartition);
        currentUser.UserGuid.Returns(admin.Guid);
        userRepository.GetByGuid(admin.Guid, Arg.Any<CancellationToken>()).Returns(admin);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids => guids.SequenceEqual(new[] { globalPartition.Guid })),
                true,
                Arg.Any<CancellationToken>())
            .Returns([globalPartition.Guid, accessibleChildGuid]);
        var sut = new ArticlesQueryHandler(
            new ActorService(userRepository, partitionRepository),
            articleRepository,
            currentUser);

        await sut.Handle(new ArticlesQuery(
            new Pagination(Page.FirstPage, Limit.MaxLimit),
            InsideAnyOfThisPartitions: [accessibleChildGuid, inaccessiblePartitionGuid]));

        await articleRepository.Received(1).GetManyInfo(
            Arg.Any<Pagination>(),
            null,
            Arg.Is<IReadOnlyCollection<Guid>>(guids =>
                guids.Count == 1 &&
                guids.Contains(accessibleChildGuid)),
            null,
            Arg.Any<CancellationToken>());
    }

    private static Partition CreatePartition(Guid guid, string name)
        => new()
        {
            Guid = guid,
            Name = new Name(name)
        };
}
