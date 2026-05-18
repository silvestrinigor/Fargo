using Fargo.Application.Identity;
using Fargo.Application.Workspaces;
using Fargo.Core;
using Fargo.Core.Workspaces;
using NSubstitute;
using System.Text.Json;

namespace Fargo.Application.Tests.Workspaces;

public sealed class WorkspaceApplicationServiceTests
{
    [Fact]
    public async Task QueueCommand_ShouldReserveArticleGuid_AndNotDispatchCommand()
    {
        var fixture = new Fixture();
        var workspaceGuid = await fixture.Sut.Begin();

        var result = await fixture.Sut.QueueCommand(
            workspaceGuid,
            new WorkspaceCommandDraft(
                "cmd-1",
                WorkspaceCommandTypes.ArticleCreate,
                1,
                """{"name":"Article"}"""));

        var workspace = await fixture.Repository.GetFoundByGuid(workspaceGuid);
        var command = Assert.Single(workspace.Commands);

        Assert.NotNull(result.ReservedEntityGuid);
        Assert.Equal(result.ReservedEntityGuid, command.ReservedEntityGuid);
        Assert.Equal(WorkspaceReservedEntityKind.Article, command.ReservedEntityKind);
        Assert.Contains(result.ReservedEntityGuid!.Value.ToString("D"), command.PayloadJson);
        Assert.Empty(fixture.Dispatcher.DispatchedCommands);
    }

    [Fact]
    public async Task Commit_ShouldDispatchQueuedCommandsInSequence()
    {
        var fixture = new Fixture();
        var workspaceGuid = await fixture.Sut.Begin();
        var article = await fixture.Sut.QueueCommand(
            workspaceGuid,
            new WorkspaceCommandDraft(
                "cmd-1",
                WorkspaceCommandTypes.ArticleCreate,
                1,
                """{"name":"Article"}"""));
        await fixture.Sut.QueueCommand(
            workspaceGuid,
            new WorkspaceCommandDraft(
                "cmd-2",
                WorkspaceCommandTypes.ArticleRename,
                1,
                JsonSerializer.Serialize(new ArticleRenameWorkspaceCommand(
                    article.ReservedEntityGuid!.Value,
                    "Renamed"))));

        var result = await fixture.Sut.Commit(workspaceGuid);

        Assert.Equal(
            ["ArticleCreateCommand", "ArticleRenameCommand"],
            fixture.Dispatcher.DispatchedCommands.Select(static command => command.GetType().Name).ToArray());
        Assert.Equal(6, fixture.UnitOfWork.SaveChangesCount);
        Assert.True(fixture.UnitOfWork.TransactionCommitted);
        fixture.ReservedGuidSession.ValidateArticleGuid(article.ReservedEntityGuid!.Value);
        Assert.Collection(
            result.Commands,
            command =>
            {
                Assert.Equal("cmd-1", command.CommandId);
                Assert.Equal(article.ReservedEntityGuid, command.EntityGuid);
            },
            command =>
            {
                Assert.Equal("cmd-2", command.CommandId);
                Assert.Null(command.EntityGuid);
            });
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            CurrentAuthorizationContext
                .GetAsync(Arg.Any<CancellationToken>())
                .Returns(new AuthorizationContext(
                    Guid.NewGuid(),
                    IsAuthenticated: true,
                    IsAdmin: true,
                    PermissionActions: [ActionType.CreateArticle],
                    PartitionAccesses: [],
                    UserGroupGuids: []));

            Sut = new WorkspaceApplicationService(
                Repository,
                CurrentAuthorizationContext,
                Dispatcher,
                ReservedGuidSession,
                UnitOfWork);
        }

        public InMemoryWorkspaceRepository Repository { get; } = new();

        public ICurrentAuthorizationContext CurrentAuthorizationContext { get; } =
            Substitute.For<ICurrentAuthorizationContext>();

        public RecordingCommandDispatcher Dispatcher { get; } = new();

        public IReservedGuidSession ReservedGuidSession { get; } = new ReservedGuidSession();

        public RecordingUnitOfWork UnitOfWork { get; } = new();

        public WorkspaceApplicationService Sut { get; }
    }

    private sealed class InMemoryWorkspaceRepository : IWorkspaceRepository
    {
        private readonly Dictionary<Guid, Workspace> workspaces = [];

        public void Add(Workspace workspace)
            => workspaces.Add(workspace.Guid, workspace);

        public Task<Workspace?> GetByGuid(Guid workspaceGuid, CancellationToken cancellationToken = default)
            => Task.FromResult(workspaces.GetValueOrDefault(workspaceGuid));

        public async Task<Workspace> GetFoundByGuid(Guid workspaceGuid, CancellationToken cancellationToken = default)
            => await GetByGuid(workspaceGuid, cancellationToken)
                ?? throw new WorkspaceNotFoundFargoApplicationException(workspaceGuid);

        public async Task<int> GetNextSequence(Guid workspaceGuid, CancellationToken cancellationToken = default)
        {
            var workspace = await GetFoundByGuid(workspaceGuid, cancellationToken);
            return workspace.Commands.Count == 0
                ? 1
                : workspace.Commands.Max(static command => command.Sequence) + 1;
        }
    }

    private sealed class RecordingCommandDispatcher : ICommandDispatcher
    {
        public List<ICommand> DispatchedCommands { get; } = [];

        public Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand
        {
            DispatchedCommands.Add(command);
            return Task.CompletedTask;
        }

        public Task<TResponse> Dispatch<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResponse>
        {
            DispatchedCommands.Add(command);

            return typeof(TResponse) == typeof(Guid) && command.GetType().GetProperty("ArticleGuid")?.GetValue(command) is Guid guid
                ? Task.FromResult((TResponse)(object)guid)
                : Task.FromResult(default(TResponse)!);
        }

        public Task<object?> Dispatch(ICommand command, CancellationToken cancellationToken = default)
        {
            DispatchedCommands.Add(command);

            var returnsGuid = command
                .GetType()
                .GetInterfaces()
                .Any(static i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICommand<>) &&
                    i.GetGenericArguments()[0] == typeof(Guid));

            return Task.FromResult(returnsGuid
                ? command.GetType().GetProperty("ArticleGuid")?.GetValue(command)
                : null);
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public bool TransactionCommitted { get; private set; }

        public Task<IUnitOfWorkTransaction> BeginTransaction(CancellationToken cancellationToken = default)
            => Task.FromResult<IUnitOfWorkTransaction>(new RecordingTransaction(this));

        public Task<int> SaveChanges(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        private sealed class RecordingTransaction(RecordingUnitOfWork unitOfWork) : IUnitOfWorkTransaction
        {
            public Task Commit(CancellationToken cancellationToken = default)
            {
                unitOfWork.TransactionCommitted = true;
                return Task.CompletedTask;
            }

            public Task Rollback(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public ValueTask DisposeAsync()
                => ValueTask.CompletedTask;
        }
    }
}
