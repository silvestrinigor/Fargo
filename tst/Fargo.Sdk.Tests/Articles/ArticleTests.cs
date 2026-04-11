using Fargo.Sdk.Articles;
using Fargo.Sdk.Partitions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Articles;

public sealed class ArticleTests
{
    private static readonly Guid ArticleGuid = Guid.NewGuid();
    private readonly IArticleClient client = Substitute.For<IArticleClient>();
    private readonly Article sut;

    public ArticleTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new Article(ArticleGuid, "Original Name", "Original Description", client, NullLogger.Instance);
    }

    // --- Name property ---

    [Fact]
    public async Task Name_Setter_Should_CallUpdateAsync_When_ValueChanges()
    {
        // Act
        sut.Name = "New Name";
        await Task.Yield(); // allow fire-and-forget to complete

        // Assert
        await client.Received(1).UpdateAsync(ArticleGuid, "New Name", "Original Description", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Name_Setter_Should_NotCallUpdateAsync_When_ValueIsUnchanged()
    {
        // Act
        sut.Name = "Original Name";
        await Task.Yield();

        // Assert
        await client.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Name_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Act
        sut.Name = "New Name";

        // Assert
        Assert.Equal("New Name", sut.Name);
    }

    // --- Description property ---

    [Fact]
    public async Task Description_Setter_Should_CallUpdateAsync_When_ValueChanges()
    {
        // Act
        sut.Description = "New Description";
        await Task.Yield();

        // Assert
        await client.Received(1).UpdateAsync(ArticleGuid, "Original Name", "New Description", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Description_Setter_Should_NotCallUpdateAsync_When_ValueIsUnchanged()
    {
        // Act
        sut.Description = "Original Description";
        await Task.Yield();

        // Assert
        await client.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Description_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Act
        sut.Description = "New Description";

        // Assert
        Assert.Equal("New Description", sut.Description);
    }

    // --- Both properties changed ---

    [Fact]
    public async Task Setting_Both_Properties_Should_CallUpdateAsync_Twice()
    {
        // Act
        sut.Name = "New Name";
        sut.Description = "New Description";
        await Task.Yield();

        // Assert
        await client.Received(2).UpdateAsync(ArticleGuid, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    // --- Guid property ---

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(ArticleGuid, sut.Guid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        // Arrange
        ArticleUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        // Act
        sut.RaiseUpdated();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(ArticleGuid, received.Guid);
    }

    [Fact]
    public void Updated_Should_NotFire_When_NoHandlerIsAttached()
    {
        // Act / Assert — must not throw
        sut.RaiseUpdated();
    }

    // --- Deleted event ---

    [Fact]
    public void Deleted_Should_Fire_When_RaiseDeletedIsCalled()
    {
        // Arrange
        ArticleDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        // Act
        sut.RaiseDeleted();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(ArticleGuid, received.Guid);
    }

    [Fact]
    public void Deleted_Should_NotFire_When_NoHandlerIsAttached()
    {
        // Act / Assert — must not throw
        sut.RaiseDeleted();
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_DelegateToClient()
    {
        // Arrange
        IReadOnlyCollection<PartitionResult> partitions = [new PartitionResult(Guid.NewGuid(), "P1", "desc", null, true)];
        client.GetPartitionsAsync(ArticleGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(partitions));

        // Act
        var result = await sut.GetPartitionsAsync();

        // Assert
        await client.Received(1).GetPartitionsAsync(ArticleGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }
}
