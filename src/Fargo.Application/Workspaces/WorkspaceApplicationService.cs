using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Workspaces;
using System.Text.Json;
using AppArticleCreateCommand = Fargo.Application.Articles.ArticleCreateCommand;
using AppArticleRenameCommand = Fargo.Application.Articles.ArticleRenameCommand;
using AppItemCreateCommand = Fargo.Application.Items.ItemCreateCommand;
using AppItemMoveToContainerCommand = Fargo.Application.Items.ItemSetParentContainerCommand;
using AppPartitionCreateCommand = Fargo.Application.Partitions.PartitionCreateCommand;
using AppPartitionMoveCommand = Fargo.Application.Partitions.PartitionSetParentCommand;
using AppPartitionRenameCommand = Fargo.Application.Partitions.PartitionRenameCommand;

namespace Fargo.Application.Workspaces;

public sealed class WorkspaceApplicationService(
    IWorkspaceRepository workspaceRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ICommandDispatcher commandDispatcher,
    IReservedGuidSession reservedGuidSession,
    IUnitOfWork unitOfWork)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid> Begin(CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        var workspace = Workspace.Begin(actor.ActorGuid);

        workspaceRepository.Add(workspace);

        await unitOfWork.SaveChanges(cancellationToken);

        return workspace.Guid;
    }

    public async Task<WorkspaceQueueCommandResult> QueueCommand(
        Guid workspaceGuid,
        WorkspaceCommandDraft draft,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetFoundByGuid(workspaceGuid, cancellationToken);

        workspace.ValidateIsPending();

        var sequence = await workspaceRepository.GetNextSequence(workspaceGuid, cancellationToken);

        var normalized = NormalizeQueuedCommand(draft);

        workspace.QueueCommand(
            draft.CommandId,
            sequence,
            draft.CommandType,
            draft.CommandVersion,
            normalized.PayloadJson,
            normalized.ReservedEntityGuid,
            normalized.ReservedEntityKind);

        await unitOfWork.SaveChanges(cancellationToken);

        return new WorkspaceQueueCommandResult(
            workspace.Guid,
            draft.CommandId,
            sequence,
            normalized.ReservedEntityGuid);
    }

    public async Task<WorkspaceCommitResult> Commit(
        Guid workspaceGuid,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetFoundByGuid(workspaceGuid, cancellationToken);

        workspace.ValidateIsPending();

        var results = new List<WorkspaceCommittedCommandResult>();

        await using var transaction = await unitOfWork.BeginTransaction(cancellationToken);

        try
        {
            RegisterReservedGuids(workspace.Commands);

            foreach (var command in workspace.Commands.OrderBy(static c => c.Sequence))
            {
                var applicationCommand = ToApplicationCommand(command);

                var response = await commandDispatcher.Dispatch(applicationCommand, cancellationToken);

                command.MarkExecuted();

                await unitOfWork.SaveChanges(cancellationToken);

                results.Add(new WorkspaceCommittedCommandResult(
                    command.CommandId,
                    command.Sequence,
                    response as Guid? ?? command.ReservedEntityGuid));
            }

            workspace.MarkCommitted();

            await unitOfWork.SaveChanges(cancellationToken);

            await transaction.Commit(cancellationToken);
        }
        catch
        {
            await transaction.Rollback(cancellationToken);

            throw;
        }

        return new WorkspaceCommitResult(workspace.Guid, results);
    }

    public async Task Rollback(
        Guid workspaceGuid,
        CancellationToken cancellationToken = default)
    {
        var workspace = await workspaceRepository.GetFoundByGuid(workspaceGuid, cancellationToken);

        workspace.MarkRolledBack();

        await unitOfWork.SaveChanges(cancellationToken);
    }

    private static NormalizedQueuedCommand NormalizeQueuedCommand(WorkspaceCommandDraft draft)
        => draft.CommandType switch
        {
            WorkspaceCommandTypes.ArticleCreate => ReserveArticleGuid(draft.PayloadJson),
            WorkspaceCommandTypes.ItemCreate => ReserveItemGuid(draft.PayloadJson),
            WorkspaceCommandTypes.PartitionCreate => ReservePartitionGuid(draft.PayloadJson),
            WorkspaceCommandTypes.ArticleRename or
            WorkspaceCommandTypes.ItemMoveToContainer or
            WorkspaceCommandTypes.PartitionRename or
            WorkspaceCommandTypes.PartitionMove => new NormalizedQueuedCommand(draft.PayloadJson, null, null),
            _ => throw new ArgumentException($"Unsupported workspace command type '{draft.CommandType}'.", nameof(draft))
        };

    private void RegisterReservedGuids(IEnumerable<WorkspaceCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command.ReservedEntityGuid is not { } reservedEntityGuid)
            {
                continue;
            }

            switch (command.ReservedEntityKind)
            {
                case WorkspaceReservedEntityKind.Article:
                    reservedGuidSession.RegisterArticleGuid(reservedEntityGuid);
                    break;
                case WorkspaceReservedEntityKind.Item:
                    reservedGuidSession.RegisterItemGuid(reservedEntityGuid);
                    break;
                case WorkspaceReservedEntityKind.Partition:
                    reservedGuidSession.RegisterPartitionGuid(reservedEntityGuid);
                    break;
                default:
                    throw new ArgumentException(
                        $"Workspace command '{command.CommandId}' has reserved GUID '{reservedEntityGuid}' without a reserved entity kind.");
            }
        }
    }

    private static ICommand ToApplicationCommand(WorkspaceCommand command)
        => command.CommandType switch
        {
            WorkspaceCommandTypes.ArticleCreate => ToArticleCreateCommand(command),
            WorkspaceCommandTypes.ArticleRename => ToArticleRenameCommand(command.PayloadJson),
            WorkspaceCommandTypes.ItemCreate => ToItemCreateCommand(command),
            WorkspaceCommandTypes.ItemMoveToContainer => ToItemMoveToContainerCommand(command.PayloadJson),
            WorkspaceCommandTypes.PartitionCreate => ToPartitionCreateCommand(command),
            WorkspaceCommandTypes.PartitionRename => ToPartitionRenameCommand(command.PayloadJson),
            WorkspaceCommandTypes.PartitionMove => ToPartitionMoveCommand(command.PayloadJson),
            _ => throw new ArgumentException($"Unsupported workspace command type '{command.CommandType}'.", nameof(command))
        };

    private static NormalizedQueuedCommand ReserveArticleGuid(string payloadJson)
    {
        var input = Deserialize<ArticleCreateWorkspaceCommandInput>(payloadJson);
        var payload = new ArticleCreateWorkspaceCommand(Guid.NewGuid(), input.Name);

        return new NormalizedQueuedCommand(Serialize(payload), payload.ArticleGuid, WorkspaceReservedEntityKind.Article);
    }

    private static NormalizedQueuedCommand ReserveItemGuid(string payloadJson)
    {
        var input = Deserialize<ItemCreateWorkspaceCommandInput>(payloadJson);
        var payload = new ItemCreateWorkspaceCommand(Guid.NewGuid(), input.ArticleGuid, input.ProductionDate);

        return new NormalizedQueuedCommand(Serialize(payload), payload.ItemGuid, WorkspaceReservedEntityKind.Item);
    }

    private static NormalizedQueuedCommand ReservePartitionGuid(string payloadJson)
    {
        var input = Deserialize<PartitionCreateWorkspaceCommandInput>(payloadJson);
        var payload = new PartitionCreateWorkspaceCommand(Guid.NewGuid(), input.Name);

        return new NormalizedQueuedCommand(Serialize(payload), payload.PartitionGuid, WorkspaceReservedEntityKind.Partition);
    }

    private static AppArticleCreateCommand ToArticleCreateCommand(WorkspaceCommand command)
    {
        var payload = Deserialize<ArticleCreateWorkspaceCommand>(command.PayloadJson);
        ValidateReservedEntityGuid(command, payload.ArticleGuid, WorkspaceReservedEntityKind.Article);

        return new AppArticleCreateCommand(new Name(payload.Name), new ReservedArticleGuid(payload.ArticleGuid));
    }

    private static AppArticleRenameCommand ToArticleRenameCommand(string payloadJson)
    {
        var payload = Deserialize<ArticleRenameWorkspaceCommand>(payloadJson);
        return new AppArticleRenameCommand(payload.ArticleGuid, new Name(payload.Name));
    }

    private static AppItemCreateCommand ToItemCreateCommand(WorkspaceCommand command)
    {
        var payload = Deserialize<ItemCreateWorkspaceCommand>(command.PayloadJson);
        ValidateReservedEntityGuid(command, payload.ItemGuid, WorkspaceReservedEntityKind.Item);

        return new AppItemCreateCommand(payload.ArticleGuid, payload.ProductionDate, new ReservedItemGuid(payload.ItemGuid));
    }

    private static AppItemMoveToContainerCommand ToItemMoveToContainerCommand(string payloadJson)
    {
        var payload = Deserialize<ItemMoveToContainerWorkspaceCommand>(payloadJson);
        return new AppItemMoveToContainerCommand(payload.ItemGuid, payload.ParentContainerGuid);
    }

    private static AppPartitionCreateCommand ToPartitionCreateCommand(WorkspaceCommand command)
    {
        var payload = Deserialize<PartitionCreateWorkspaceCommand>(command.PayloadJson);
        ValidateReservedEntityGuid(command, payload.PartitionGuid, WorkspaceReservedEntityKind.Partition);

        return new AppPartitionCreateCommand(new Name(payload.Name), new ReservedPartitionGuid(payload.PartitionGuid));
    }

    private static AppPartitionRenameCommand ToPartitionRenameCommand(string payloadJson)
    {
        var payload = Deserialize<PartitionRenameWorkspaceCommand>(payloadJson);
        return new AppPartitionRenameCommand(payload.PartitionGuid, new Name(payload.Name));
    }

    private static AppPartitionMoveCommand ToPartitionMoveCommand(string payloadJson)
    {
        var payload = Deserialize<PartitionMoveWorkspaceCommand>(payloadJson);
        return new AppPartitionMoveCommand(payload.PartitionGuid, payload.ParentPartitionGuid);
    }

    private static T Deserialize<T>(string payloadJson)
        => JsonSerializer.Deserialize<T>(payloadJson, JsonOptions)
            ?? throw new ArgumentException($"Command payload could not be deserialized as '{typeof(T).Name}'.");

    private static string Serialize<T>(T payload)
        => JsonSerializer.Serialize(payload, JsonOptions);

    private static void ValidateReservedEntityGuid(
        WorkspaceCommand command,
        Guid payloadGuid,
        WorkspaceReservedEntityKind expectedKind)
    {
        if (command.ReservedEntityGuid != payloadGuid || command.ReservedEntityKind != expectedKind)
        {
            throw new ArgumentException(
                $"Workspace command '{command.CommandId}' payload GUID does not match its reserved {expectedKind} GUID.");
        }
    }

    private sealed record NormalizedQueuedCommand(
        string PayloadJson,
        Guid? ReservedEntityGuid,
        WorkspaceReservedEntityKind? ReservedEntityKind);

    private sealed record ArticleCreateWorkspaceCommandInput(string Name);

    private sealed record ItemCreateWorkspaceCommandInput(
        Guid ArticleGuid,
        DateTimeOffset? ProductionDate);

    private sealed record PartitionCreateWorkspaceCommandInput(string Name);
}
