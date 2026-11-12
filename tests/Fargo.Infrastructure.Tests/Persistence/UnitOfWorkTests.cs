using Fargo.Application.Persistence;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    // The in-memory provider has no real transactions; it still shows what is saved.
    private FargoDbContext CreateContext()
        => new(new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldReturnTheResultAndSave()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context, NullLogger<IUnitOfWork>.Instance);

        var result = await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Articles.Add(Article.NewArticle(new Name("Widget")));

            return Task.FromResult(42);
        });

        Assert.Equal(42, result);
        await using var check = CreateContext();
        Assert.Equal(1, await check.Articles.CountAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenCancelledAfterTheWork_ShouldStillSave()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context, NullLogger<IUnitOfWork>.Instance);
        using var stopping = new CancellationTokenSource();

        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Articles.Add(Article.NewArticle(new Name("Widget")));

            // Cancellation arrives after the work finished, as on host shutdown.
            stopping.Cancel();

            return Task.CompletedTask;
        }, stopping.Token);

        await using var check = CreateContext();
        Assert.Equal(1, await check.Articles.CountAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheWorkThrows_ShouldSaveNothing()
    {
        await using var context = CreateContext();
        var unitOfWork = new UnitOfWork(context, NullLogger<IUnitOfWork>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Articles.Add(Article.NewArticle(new Name("Widget")));

            throw new InvalidOperationException("boom");
        }));

        await using var check = CreateContext();
        Assert.Equal(0, await check.Articles.CountAsync());
    }
}
