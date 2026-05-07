using Fargo.Application;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Repositories;

public sealed class PartitionedRepositoryFilterTests
{
    [Fact]
    public async Task ArticleGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = new Article { Name = new Name("Public article") };
        var firstPartitionEntity = new Article { Name = new Name("First article") };
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = new Article { Name = new Name("Second article") };
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Articles.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ArticleRepository(context);

        await AssertFilterCombinations(
            (partitions, notInsideAnyPartition) => repository.GetManyInfo(
                AllRows,
                insideAnyOfThisPartitions: partitions,
                notInsideAnyPartition: notInsideAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task ItemGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var article = new Article { Name = new Name("Article") };
        var publicEntity = new Item(article);
        var firstPartitionEntity = new Item(article);
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = new Item(article);
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Articles.Add(article);
        context.Items.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);

        await AssertFilterCombinations(
            (partitions, notInsideAnyPartition) => repository.GetManyInfo(
                AllRows,
                insideAnyOfThisPartitions: partitions,
                notInsideAnyPartition: notInsideAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = CreateUser("public-user");
        var firstPartitionEntity = CreateUser("first-user");
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = CreateUser("second-user");
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.Users.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        await AssertFilterCombinations(
            (partitions, notInsideAnyPartition) => repository.GetManyInfo(
                AllRows,
                insideAnyOfThisPartitions: partitions,
                notInsideAnyPartition: notInsideAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    [Fact]
    public async Task UserGroupGetManyInfo_Should_ApplyPartitionFilterCombinations()
    {
        await using var context = CreateContext();
        var (firstPartition, secondPartition) = AddPartitions(context);
        var publicEntity = new UserGroup { Nameid = new Nameid("public-group") };
        var firstPartitionEntity = new UserGroup { Nameid = new Nameid("first-group") };
        firstPartitionEntity.Partitions.Add(firstPartition);
        var secondPartitionEntity = new UserGroup { Nameid = new Nameid("second-group") };
        secondPartitionEntity.Partitions.Add(secondPartition);
        context.UserGroups.AddRange(publicEntity, firstPartitionEntity, secondPartitionEntity);
        await context.SaveChangesAsync();

        var repository = new UserGroupRepository(context);

        await AssertFilterCombinations(
            (partitions, notInsideAnyPartition) => repository.GetManyInfo(
                AllRows,
                insideAnyOfThisPartitions: partitions,
                notInsideAnyPartition: notInsideAnyPartition),
            publicEntity.Guid,
            firstPartitionEntity.Guid,
            secondPartitionEntity.Guid,
            firstPartition.Guid);
    }

    private static readonly Pagination AllRows = new(Page.FirstPage, Limit.MaxLimit);

    private static async Task AssertFilterCombinations<T>(
        Func<IReadOnlyCollection<Guid>?, bool?, Task<IReadOnlyCollection<T>>> query,
        Guid publicEntityGuid,
        Guid firstPartitionEntityGuid,
        Guid secondPartitionEntityGuid,
        Guid firstPartitionGuid)
        where T : class
    {
        AssertGuids([publicEntityGuid, firstPartitionEntityGuid, secondPartitionEntityGuid], await query(null, null));
        AssertGuids([publicEntityGuid], await query(null, true));
        AssertGuids([firstPartitionEntityGuid, secondPartitionEntityGuid], await query(null, false));
        AssertGuids([firstPartitionEntityGuid], await query([firstPartitionGuid], null));
        AssertGuids([publicEntityGuid, firstPartitionEntityGuid], await query([firstPartitionGuid], true));
        AssertGuids([], await query([], null));
        AssertGuids([publicEntityGuid], await query([], true));
    }

    private static void AssertGuids<T>(IReadOnlyCollection<Guid> expected, IReadOnlyCollection<T> actual)
        where T : class
    {
        var actualGuids = actual
            .Select(GetGuid)
            .Order()
            .ToArray();

        Assert.Equal(expected.Order(), actualGuids);
    }

    private static Guid GetGuid<T>(T dto)
        => dto switch
        {
            Fargo.Application.Articles.ArticleDto article => article.Guid,
            Fargo.Application.Items.ItemDto item => item.Guid,
            Fargo.Application.Users.UserDto user => user.Guid,
            Fargo.Application.UserGroups.UserGroupDto userGroup => userGroup.Guid,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unsupported DTO type.")
        };

    private static FargoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FargoDbContext(options);
    }

    private static (Partition First, Partition Second) AddPartitions(FargoDbContext context)
    {
        var first = new Partition { Name = new Name("First partition") };
        var second = new Partition { Name = new Name("Second partition") };
        context.Partitions.AddRange(first, second);
        return (first, second);
    }

    private static User CreateUser(string nameid)
        => new()
        {
            Nameid = new Nameid(nameid),
            PasswordHash = new PasswordHash(new string('h', PasswordHash.MinLength))
        };
}
