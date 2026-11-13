using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Articles;
using Fargo.Application.Decorators;
using Fargo.Application.Dtos.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Common;

public class AuditCommandHandlerDecoratorTests
{
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();

    private static ArticleCreateCommand Command() => new(new ArticleCreateDto(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenInnerSucceedsAndActorAuthenticated_RecordsAudit()
    {
        var actorGuid = Guid.NewGuid();
        var currentActor = new TestCurrentActor { Guid = actorGuid, ActorType = ActorType.User, IsAuthenticated = true };
        var inner = Substitute.For<ICommandHandler<ArticleCreateCommand>>();

        var decorator = new AuditCommandHandlerDecorator<ArticleCreateCommand>(
            inner, currentActor, auditLogRepository);

        await decorator.HandleAsync(Command());

        auditLogRepository.Received(1).Add(Arg.Is<AuditLog>(a =>
            a!.ActorGuid == actorGuid
            && a.ActorType == ActorType.User
            && a.AuditLogType == AuditLogType.EntityCreated
            && a.CommandName == nameof(ArticleCreateCommand)));
    }

    [Fact]
    public async Task HandleAsync_WhenInnerThrows_DoesNotRecordAuditAndPropagates()
    {
        var currentActor = new TestCurrentActor { IsAuthenticated = true };
        var inner = Substitute.For<ICommandHandler<ArticleCreateCommand>>();
        inner.HandleAsync(Arg.Any<ArticleCreateCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var decorator = new AuditCommandHandlerDecorator<ArticleCreateCommand>(
            inner, currentActor, auditLogRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.HandleAsync(Command()));

        auditLogRepository.DidNotReceive().Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoAuthenticatedActor_DoesNotRecordAuditButRunsInner()
    {
        var inner = Substitute.For<ICommandHandler<ArticleCreateCommand>>();

        // No current actor (actor-less host, e.g. initializer worker).
        var decorator = new AuditCommandHandlerDecorator<ArticleCreateCommand>(
            inner, currentActor: null, auditLogRepository);

        await decorator.HandleAsync(Command());

        await inner.Received(1).HandleAsync(Arg.Any<ArticleCreateCommand>(), Arg.Any<CancellationToken>());
        auditLogRepository.DidNotReceive().Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorPresentButNotAuthenticated_DoesNotRecordAudit()
    {
        var currentActor = new TestCurrentActor { IsAuthenticated = false };
        var inner = Substitute.For<ICommandHandler<ArticleCreateCommand>>();

        var decorator = new AuditCommandHandlerDecorator<ArticleCreateCommand>(
            inner, currentActor, auditLogRepository);

        await decorator.HandleAsync(Command());

        auditLogRepository.DidNotReceive().Add(Arg.Any<AuditLog>());
    }
}
