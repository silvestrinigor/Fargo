using Fargo.Api.ApiClients;
using NSubstitute;

namespace Fargo.Api.Tests.ApiClients;

public sealed class ApiClientTests
{
    private static readonly Guid ApiClientGuid = Guid.NewGuid();
    private readonly IApiClientHttpClient client = Substitute.For<IApiClientHttpClient>();
    private readonly ApiClient sut;

    public ApiClientTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        client.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new ApiClient(ApiClientGuid, "test-client", "Original Description", true, client);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_CallClientUpdateAsync_With_ProvidedValues()
    {
        var result = await sut.UpdateAsync(x => x.Name = "New Name");

        await client.Received(1).UpdateAsync(ApiClientGuid, "New Name", Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateLocalValues()
    {
        await sut.UpdateAsync(x =>
        {
            x.Name = "New Name";
            x.Description = "New Description";
        });

        Assert.Equal("New Name", sut.Name);
        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFailure_When_UpdateFails()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "API client not found.")));

        var result = await sut.UpdateAsync(x => x.Name = "New Name");

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClientDeleteAsync()
    {
        var result = await sut.DeleteAsync();

        await client.Received(1).DeleteAsync(ApiClientGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFailure_When_DeleteFails()
    {
        client.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "API client not found.")));

        var result = await sut.DeleteAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- Property getters ---

    [Fact]
    public void Name_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Name = "New Name";

        Assert.Equal("New Name", sut.Name);
    }

    [Fact]
    public void Description_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Description = "New Description";

        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public void IsActive_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.IsActive = false;

        Assert.False(sut.IsActive);
    }

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(ApiClientGuid, sut.Guid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        ApiClientUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        sut.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(ApiClientGuid, received.Guid);
    }

    [Fact]
    public void Updated_Should_NotFire_When_NoHandlerIsAttached()
    {
        sut.RaiseUpdated();
    }

    // --- Deleted event ---

    [Fact]
    public void Deleted_Should_Fire_When_RaiseDeletedIsCalled()
    {
        ApiClientDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        sut.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(ApiClientGuid, received.Guid);
    }

    [Fact]
    public void Deleted_Should_NotFire_When_NoHandlerIsAttached()
    {
        sut.RaiseDeleted();
    }
}
