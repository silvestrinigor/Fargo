using Fargo.Sdk.Articles;
using Fargo.Sdk.Http;
using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests;

public sealed class ClientQueryStringTests
{
    private readonly IFargoHttpClient httpClient = Substitute.For<IFargoHttpClient>();

    [Fact]
    public async Task ArticleGetManyAsync_Should_SendApiCanonicalQueryParameters()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Articles.ArticleInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Empty<Fargo.Sdk.Contracts.Articles.ArticleInfo>());

        var client = new ArticleHttpClient(httpClient);
        var firstPartitionGuid = Guid.NewGuid();
        var secondPartitionGuid = Guid.NewGuid();
        var temporalAsOf = DateTimeOffset.Parse("2026-05-06T10:15:30+00:00");

        await client.GetManyAsync(temporalAsOf, 2, 25, [firstPartitionGuid, secondPartitionGuid], true);

        await httpClient.Received().GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Articles.ArticleInfo>>(
            Arg.Is<string>(path =>
                path.StartsWith("/articles?", StringComparison.Ordinal) &&
                path.Contains("temporalAsOfDateTime=", StringComparison.Ordinal) &&
                path.Contains("page=2", StringComparison.Ordinal) &&
                path.Contains("limit=25", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={firstPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={secondPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains("notInsideAnyPartition=True", StringComparison.Ordinal) &&
                !path.Contains("partitionGuid=", StringComparison.Ordinal) &&
                !path.Contains("noPartition=", StringComparison.Ordinal) &&
                !path.Contains("search=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ItemGetManyAsync_Should_SendApiCanonicalQueryParameters()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Empty<Fargo.Sdk.Contracts.Items.ItemInfo>());

        var client = new ItemHttpClient(httpClient);
        var firstPartitionGuid = Guid.NewGuid();
        var secondPartitionGuid = Guid.NewGuid();

        await client.GetManyAsync(DateTimeOffset.UtcNow, 3, 50, [firstPartitionGuid, secondPartitionGuid], true);

        await httpClient.Received().GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(
            Arg.Is<string>(path =>
                path.StartsWith("/items?", StringComparison.Ordinal) &&
                path.Contains("temporalAsOfDateTime=", StringComparison.Ordinal) &&
                path.Contains("page=3", StringComparison.Ordinal) &&
                path.Contains("limit=50", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={firstPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={secondPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains("notInsideAnyPartition=True", StringComparison.Ordinal) &&
                !path.Contains("articleGuid=", StringComparison.Ordinal) &&
                !path.Contains("partitionGuid=", StringComparison.Ordinal) &&
                !path.Contains("noPartition=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserGetManyAsync_Should_SendApiCanonicalQueryParameters()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Users.UserInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Empty<Fargo.Sdk.Contracts.Users.UserInfo>());

        var client = new UserHttpClient(httpClient);
        var firstPartitionGuid = Guid.NewGuid();
        var secondPartitionGuid = Guid.NewGuid();

        await client.GetManyAsync(DateTimeOffset.UtcNow, 4, 10, [firstPartitionGuid, secondPartitionGuid], true);

        await httpClient.Received().GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Users.UserInfo>>(
            Arg.Is<string>(path =>
                path.StartsWith("/users?", StringComparison.Ordinal) &&
                path.Contains("temporalAsOfDateTime=", StringComparison.Ordinal) &&
                path.Contains("page=4", StringComparison.Ordinal) &&
                path.Contains("limit=10", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={firstPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={secondPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains("notInsideAnyPartition=True", StringComparison.Ordinal) &&
                !path.Contains("partitionGuid=", StringComparison.Ordinal) &&
                !path.Contains("noPartition=", StringComparison.Ordinal) &&
                !path.Contains("search=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserGroupGetManyAsync_Should_SendApiCanonicalQueryParameters()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Empty<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>());

        var client = new UserGroupHttpClient(httpClient);
        var firstPartitionGuid = Guid.NewGuid();
        var secondPartitionGuid = Guid.NewGuid();

        await client.GetManyAsync(DateTimeOffset.UtcNow, 5, 20, [firstPartitionGuid, secondPartitionGuid], true);

        await httpClient.Received().GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(
            Arg.Is<string>(path =>
                path.StartsWith("/user-groups?", StringComparison.Ordinal) &&
                path.Contains("temporalAsOfDateTime=", StringComparison.Ordinal) &&
                path.Contains("page=5", StringComparison.Ordinal) &&
                path.Contains("limit=20", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={firstPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains($"insideAnyOfThisPartitions={secondPartitionGuid}", StringComparison.Ordinal) &&
                path.Contains("notInsideAnyPartition=True", StringComparison.Ordinal) &&
                !path.Contains("userGuid=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PartitionGetManyAsync_Should_Not_SendUnsupportedHierarchyFilters()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Empty<Fargo.Sdk.Contracts.Partitions.PartitionInfo>());

        var client = new PartitionHttpClient(httpClient);

        await client.GetManyAsync(Guid.NewGuid(), DateTimeOffset.UtcNow, 6, 30, true, "ignored");

        await httpClient.Received().GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(
            Arg.Is<string>(path =>
                path.StartsWith("/partitions?", StringComparison.Ordinal) &&
                path.Contains("temporalAsOfDateTime=", StringComparison.Ordinal) &&
                path.Contains("page=6", StringComparison.Ordinal) &&
                path.Contains("limit=30", StringComparison.Ordinal) &&
                !path.Contains("parentPartitionGuid=", StringComparison.Ordinal) &&
                !path.Contains("rootOnly=", StringComparison.Ordinal) &&
                !path.Contains("search=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    private static FargoSdkHttpResponse<IReadOnlyCollection<T>> Empty<T>()
        => new(true, [], null, HttpStatusCode.OK);
}
