using Fargo.GrpcClient;
using Fargo.GrpcContracts.Commands.V1;
using SdkCommands = Fargo.Sdk.Commands;

namespace Fargo.Sdk.Tests;

public sealed class FargoCommandClientTests
{
    [Fact]
    public async Task BeginWorkspaceAsync_Should_MapResponse()
    {
        var workspaceGuid = Guid.NewGuid();
        var transport = new FakeWorkspaceClient
        {
            BeginResponse = new BeginWorkspaceResponse
            {
                CorrelationId = "correlation",
                WorkspaceGuid = workspaceGuid.ToString("D")
            }
        };
        var sut = new FargoCommandClient(transport);

        var result = await sut.BeginWorkspaceAsync("correlation");

        Assert.Equal("correlation", transport.BeginRequest!.CorrelationId);
        Assert.Equal(workspaceGuid, result.WorkspaceGuid);
    }

    [Fact]
    public async Task QueueCommandAsync_Should_MapArticleCreateCommand()
    {
        var workspaceGuid = Guid.NewGuid();
        var reservedGuid = Guid.NewGuid();
        var transport = new FakeWorkspaceClient
        {
            QueueResponse = new QueueCommandResponse
            {
                CorrelationId = "correlation",
                WorkspaceGuid = workspaceGuid.ToString("D"),
                CommandId = "command",
                Sequence = 7,
                ReservedEntityGuid = reservedGuid.ToString("D")
            }
        };
        var sut = new FargoCommandClient(transport);

        var result = await sut.QueueCommandAsync(
            workspaceGuid,
            new SdkCommands.ArticleCreateCommand("Article"),
            commandId: "command",
            correlationId: "correlation");

        Assert.Equal(workspaceGuid.ToString("D"), transport.QueueRequest!.WorkspaceGuid);
        Assert.Equal("command", transport.QueueRequest.CommandId);
        Assert.Equal("Article", transport.QueueRequest.Command.ArticleCreate.Name);
        Assert.Equal(CommandEnvelope.PayloadOneofCase.ArticleCreate, transport.QueueRequest.Command.PayloadCase);
        Assert.Equal(reservedGuid, result.ReservedEntityGuid);
        Assert.Equal(7, result.Sequence);
    }

    [Fact]
    public async Task QueueCommandAsync_Should_MapItemMoveToContainerCommand()
    {
        var workspaceGuid = Guid.NewGuid();
        var itemGuid = Guid.NewGuid();
        var parentGuid = Guid.NewGuid();
        var transport = new FakeWorkspaceClient
        {
            QueueResponse = new QueueCommandResponse
            {
                CorrelationId = "correlation",
                WorkspaceGuid = workspaceGuid.ToString("D"),
                CommandId = "command",
                Sequence = 1
            }
        };
        var sut = new FargoCommandClient(transport);

        await sut.QueueCommandAsync(
            workspaceGuid,
            new SdkCommands.ItemMoveToContainerCommand(itemGuid, parentGuid),
            commandId: "command",
            correlationId: "correlation");

        Assert.Equal(itemGuid.ToString("D"), transport.QueueRequest!.Command.ItemMoveToContainer.ItemGuid);
        Assert.Equal(parentGuid.ToString("D"), transport.QueueRequest.Command.ItemMoveToContainer.ParentContainerGuid);
    }

    [Fact]
    public async Task CommitWorkspaceAsync_Should_MapCommittedCommands()
    {
        var workspaceGuid = Guid.NewGuid();
        var entityGuid = Guid.NewGuid();
        var transport = new FakeWorkspaceClient
        {
            CommitResponse = new CommitWorkspaceResponse
            {
                CorrelationId = "correlation",
                WorkspaceGuid = workspaceGuid.ToString("D"),
                Commands =
                {
                    new CommittedCommandResult
                    {
                        CommandId = "command",
                        Sequence = 3,
                        EntityGuid = entityGuid.ToString("D")
                    }
                }
            }
        };
        var sut = new FargoCommandClient(transport);

        var result = await sut.CommitWorkspaceAsync(workspaceGuid, "correlation");

        Assert.Equal(workspaceGuid.ToString("D"), transport.CommitRequest!.WorkspaceGuid);
        var command = Assert.Single(result.Commands);
        Assert.Equal("command", command.CommandId);
        Assert.Equal(entityGuid, command.EntityGuid);
    }

    [Fact]
    public async Task RollbackWorkspaceAsync_Should_CallTransport()
    {
        var workspaceGuid = Guid.NewGuid();
        var transport = new FakeWorkspaceClient();
        var sut = new FargoCommandClient(transport);

        await sut.RollbackWorkspaceAsync(workspaceGuid, "correlation");

        Assert.Equal(workspaceGuid.ToString("D"), transport.RollbackRequest!.WorkspaceGuid);
        Assert.Equal("correlation", transport.RollbackRequest.CorrelationId);
    }

    private sealed class FakeWorkspaceClient : IFargoWorkspaceClient
    {
        public BeginWorkspaceRequest? BeginRequest { get; private set; }
        public QueueCommandRequest? QueueRequest { get; private set; }
        public CommitWorkspaceRequest? CommitRequest { get; private set; }
        public RollbackWorkspaceRequest? RollbackRequest { get; private set; }

        public BeginWorkspaceResponse BeginResponse { get; init; } = new();
        public QueueCommandResponse QueueResponse { get; init; } = new();
        public CommitWorkspaceResponse CommitResponse { get; init; } = new();

        public Task<BeginWorkspaceResponse> BeginWorkspaceAsync(
            BeginWorkspaceRequest request,
            CancellationToken cancellationToken = default)
        {
            BeginRequest = request;
            return Task.FromResult(BeginResponse);
        }

        public Task<QueueCommandResponse> QueueCommandAsync(
            QueueCommandRequest request,
            CancellationToken cancellationToken = default)
        {
            QueueRequest = request;
            return Task.FromResult(QueueResponse);
        }

        public Task<CommitWorkspaceResponse> CommitWorkspaceAsync(
            CommitWorkspaceRequest request,
            CancellationToken cancellationToken = default)
        {
            CommitRequest = request;
            return Task.FromResult(CommitResponse);
        }

        public Task RollbackWorkspaceAsync(
            RollbackWorkspaceRequest request,
            CancellationToken cancellationToken = default)
        {
            RollbackRequest = request;
            return Task.CompletedTask;
        }
    }
}
