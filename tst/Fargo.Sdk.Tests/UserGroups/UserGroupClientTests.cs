using Fargo.Sdk.Http;
using Fargo.Sdk.UserGroups;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.UserGroups;

public sealed class UserGroupClientTests
{
    private readonly IFargoHttpClient httpClient = Substitute.For<IFargoHttpClient>();
    private readonly UserGroupHttpClient sut;

    public UserGroupClientTests()
    {
        sut = new UserGroupHttpClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var userGroupResult = Fakes.UserGroupInfo();
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(true, userGroupResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(userGroupResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(userGroupResult.Guid, result.Data!.Guid);
        Assert.Equal(userGroupResult.Nameid, result.Data.Nameid);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsUserGroupNotFound()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(false, null, Fakes.Problem("user-group/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnUserGroups_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo> groups = [Fakes.UserGroupInfo(), Fakes.UserGroupInfo()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(true, groups, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_AccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.UserGroups.UserGroupInfo>>(false, null, Fakes.Problem("entity/access-denied"), HttpStatusCode.Forbidden));

        var result = await sut.GetManyAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnGuid_When_HttpResponseIsSuccess()
    {
        var guid = Guid.NewGuid();
        var request = new Fargo.Sdk.Contracts.UserGroups.UserGroupCreateRequest("mygroup");
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupCreateRequest, Guid>(Arg.Any<string>(), Arg.Is(request), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        var result = await sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnConflict_When_NameidAlreadyExists()
    {
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupCreateRequest, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.UserGroups.UserGroupCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("user-group/nameid-already-exists"), HttpStatusCode.Conflict));

        var result = await sut.CreateAsync(new Fargo.Sdk.Contracts.UserGroups.UserGroupCreateRequest("existinggroup"));

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Conflict, result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var request = new Fargo.Sdk.Contracts.UserGroups.UserGroupUpdateRequest("newgroup");
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupUpdateRequest>(Arg.Any<string>(), Arg.Is(request), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.UpdateAsync(Guid.NewGuid(), request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_When_UserGroupDoesNotExist()
    {
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.UserGroups.UserGroupUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.UserGroups.UserGroupUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("user-group/not-found"), HttpStatusCode.NotFound));

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Fargo.Sdk.Contracts.UserGroups.UserGroupUpdateRequest("newgroup"));

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnInvalidInput_When_CannotDeleteDefaultAdmins()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("user-group/cannot-delete-default-admins"), HttpStatusCode.Conflict));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo> partitions = [Fakes.PartitionInfo()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, partitions, null, HttpStatusCode.OK));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    private static class Fakes
    {
        public static Fargo.Sdk.Contracts.UserGroups.UserGroupInfo UserGroupInfo() =>
            new(Guid.NewGuid(), "testgroup", "A test description", true, [], []);

        public static Fargo.Sdk.Contracts.Partitions.PartitionInfo PartitionInfo() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
