using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Outbox;
using Fargo.Application.Decorators;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Dtos.Outbox;
using Fargo.Application.IntegrationEvents;
using Fargo.Application.Persistence;
using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Tests.Common;
using Fargo.Core.Audits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Fargo.Application.Tests.Outbox;

public class OutboxCommandPipelineTests
{
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();

    [Fact]
    public async Task AuditDecorator_WhenTheCommandIsNotAudited_ShouldRecordNothingEvenWithAnActor()
    {
        var inner = Substitute.For<ICommandHandler<PurgeProcessedOutboxCommand, int>>();
        var decorator = new AuditCommandHandlerDecorator<PurgeProcessedOutboxCommand, int>(
            inner, new TestCurrentActor { IsAuthenticated = true }, auditLogRepository);

        await decorator.HandleAsync(new PurgeProcessedOutboxCommand(DateTimeOffset.UtcNow));

        await inner.Received(1).HandleAsync(Arg.Any<PurgeProcessedOutboxCommand>(), Arg.Any<CancellationToken>());
        auditLogRepository.DidNotReceive().Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task PublishCommand_ShouldRunInsideTheUnitOfWorkTransaction()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork
            .ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<OutboxBatchResultDto>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<OutboxBatchResultDto>>>()(CancellationToken.None));

        var repository = Substitute.For<IOutboxRepository>();
        repository
            .ClaimBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(unitOfWork);
        services.AddSingleton(repository);
        services.AddSingleton(auditLogRepository);
        services.AddSingleton(Substitute.For<IEventPublisher>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddFargoOutboxApplication();

        await using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto>>();
        var result = await handler.HandleAsync(new PublishOutboxBatchCommand(100));

        Assert.Equal(new OutboxBatchResultDto(0, 0, false), result);

        // The decorator owns the transaction: the claim ran inside it, and no plain save was used.
        await unitOfWork.Received(1).ExecuteInTransactionAsync(
            Arg.Any<Func<CancellationToken, Task<OutboxBatchResultDto>>>(),
            Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await repository.Received(1).ClaimBatchAsync(100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeCommand_ShouldUseTheCommitOnSuccessDecorator()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var repository = Substitute.For<IOutboxRepository>();

        var services = new ServiceCollection();
        services.AddSingleton(unitOfWork);
        services.AddSingleton(repository);
        services.AddSingleton(auditLogRepository);
        services.AddSingleton(Substitute.For<IEventPublisher>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddFargoOutboxApplication();

        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<ICommandHandler<PurgeProcessedOutboxCommand, int>>()
            .HandleAsync(new PurgeProcessedOutboxCommand(DateTimeOffset.UtcNow));

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceiveWithAnyArgs()
            .ExecuteInTransactionAsync<int>(default!, default);
    }
}
