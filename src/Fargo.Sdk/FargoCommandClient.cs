using Fargo.GrpcClient;
using Fargo.GrpcContracts.Commands.V1;
using Fargo.Sdk.Commands;
using Google.Protobuf.WellKnownTypes;

namespace Fargo.Sdk;

public sealed class FargoCommandClient(IFargoWorkspaceClient workspaceClient) : IFargoCommandClient
{
    public async Task<FargoWorkspace> BeginWorkspaceAsync(
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await workspaceClient.BeginWorkspaceAsync(
            new BeginWorkspaceRequest
            {
                CorrelationId = CreateCorrelationId(correlationId)
            },
            cancellationToken);

        return new FargoWorkspace(
            response.CorrelationId,
            ParseGuid(response.WorkspaceGuid, nameof(response.WorkspaceGuid)));
    }

    public async Task<FargoQueuedCommand> QueueCommandAsync(
        Guid workspaceGuid,
        FargoCommand command,
        string? commandId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await workspaceClient.QueueCommandAsync(
            new QueueCommandRequest
            {
                CorrelationId = CreateCorrelationId(correlationId),
                WorkspaceGuid = workspaceGuid.ToString("D"),
                CommandId = string.IsNullOrWhiteSpace(commandId) ? Guid.NewGuid().ToString("D") : commandId,
                Command = ToEnvelope(command)
            },
            cancellationToken);

        return new FargoQueuedCommand(
            response.CorrelationId,
            ParseGuid(response.WorkspaceGuid, nameof(response.WorkspaceGuid)),
            response.CommandId,
            response.Sequence,
            ParseOptionalGuid(response.ReservedEntityGuid, nameof(response.ReservedEntityGuid)));
    }

    public async Task<FargoWorkspaceCommit> CommitWorkspaceAsync(
        Guid workspaceGuid,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var response = await workspaceClient.CommitWorkspaceAsync(
            new CommitWorkspaceRequest
            {
                CorrelationId = CreateCorrelationId(correlationId),
                WorkspaceGuid = workspaceGuid.ToString("D")
            },
            cancellationToken);

        return new FargoWorkspaceCommit(
            response.CorrelationId,
            ParseGuid(response.WorkspaceGuid, nameof(response.WorkspaceGuid)),
            response.Commands.Select(static command => new FargoCommittedCommand(
                command.CommandId,
                command.Sequence,
                ParseOptionalGuid(command.EntityGuid, nameof(command.EntityGuid)))).ToArray());
    }

    public async Task RollbackWorkspaceAsync(
        Guid workspaceGuid,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        => await workspaceClient.RollbackWorkspaceAsync(
            new RollbackWorkspaceRequest
            {
                CorrelationId = CreateCorrelationId(correlationId),
                WorkspaceGuid = workspaceGuid.ToString("D")
            },
            cancellationToken);

    private static CommandEnvelope ToEnvelope(FargoCommand command)
        => command switch
        {
            Commands.ArticleCreateCommand articleCreate => new CommandEnvelope
            {
                ArticleCreate = new GrpcContracts.Commands.V1.ArticleCreateCommand
                {
                    Name = articleCreate.Name
                }
            },
            Commands.ArticleRenameCommand articleRename => new CommandEnvelope
            {
                ArticleRename = new GrpcContracts.Commands.V1.ArticleRenameCommand
                {
                    ArticleGuid = articleRename.ArticleGuid.ToString("D"),
                    Name = articleRename.Name
                }
            },
            Commands.ItemCreateCommand itemCreate => new CommandEnvelope
            {
                ItemCreate = new GrpcContracts.Commands.V1.ItemCreateCommand
                {
                    ArticleGuid = itemCreate.ArticleGuid.ToString("D"),
                    ProductionDate = itemCreate.ProductionDate?.ToTimestamp()
                }
            },
            Commands.ItemMoveToContainerCommand itemMoveToContainer => new CommandEnvelope
            {
                ItemMoveToContainer = new GrpcContracts.Commands.V1.ItemMoveToContainerCommand
                {
                    ItemGuid = itemMoveToContainer.ItemGuid.ToString("D"),
                    ParentContainerGuid = itemMoveToContainer.ParentContainerGuid?.ToString("D")
                }
            },
            Commands.PartitionCreateCommand partitionCreate => new CommandEnvelope
            {
                PartitionCreate = new GrpcContracts.Commands.V1.PartitionCreateCommand
                {
                    Name = partitionCreate.Name
                }
            },
            Commands.PartitionRenameCommand partitionRename => new CommandEnvelope
            {
                PartitionRename = new GrpcContracts.Commands.V1.PartitionRenameCommand
                {
                    PartitionGuid = partitionRename.PartitionGuid.ToString("D"),
                    Name = partitionRename.Name
                }
            },
            Commands.PartitionMoveCommand partitionMove => new CommandEnvelope
            {
                PartitionMove = new GrpcContracts.Commands.V1.PartitionMoveCommand
                {
                    PartitionGuid = partitionMove.PartitionGuid.ToString("D"),
                    ParentPartitionGuid = partitionMove.ParentPartitionGuid.ToString("D")
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Unsupported Fargo command.")
        };

    private static string CreateCorrelationId(string? correlationId)
        => string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("D") : correlationId;

    private static Guid ParseGuid(string value, string fieldName)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new FormatException($"Field '{fieldName}' must be a valid GUID.");
    }

    private static Guid? ParseOptionalGuid(string value, string fieldName)
        => string.IsNullOrWhiteSpace(value) ? null : ParseGuid(value, fieldName);
}
