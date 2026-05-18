using Fargo.Application.Workspaces;
using Fargo.GrpcContracts.Commands.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Text.Json;

namespace Fargo.GrpcApi.Services;

public sealed class FargoWorkspaceGrpcService(
    WorkspaceApplicationService workspaces
) : WorkspaceService.WorkspaceServiceBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override async Task<BeginWorkspaceResponse> BeginWorkspace(
        BeginWorkspaceRequest request,
        ServerCallContext context)
    {
        var workspaceGuid = await workspaces.Begin(context.CancellationToken);

        return new BeginWorkspaceResponse
        {
            CorrelationId = request.CorrelationId,
            WorkspaceGuid = workspaceGuid.ToString("D")
        };
    }

    public override async Task<QueueCommandResponse> QueueCommand(
        QueueCommandRequest request,
        ServerCallContext context)
    {
        var result = await workspaces.QueueCommand(
            ParseGuid(request.WorkspaceGuid, nameof(request.WorkspaceGuid)),
            ToDraft(request),
            context.CancellationToken);

        return new QueueCommandResponse
        {
            CorrelationId = request.CorrelationId,
            WorkspaceGuid = result.WorkspaceGuid.ToString("D"),
            CommandId = result.CommandId,
            Sequence = result.Sequence,
            ReservedEntityGuid = result.ReservedEntityGuid?.ToString("D") ?? string.Empty
        };
    }

    public override async Task<CommitWorkspaceResponse> CommitWorkspace(
        CommitWorkspaceRequest request,
        ServerCallContext context)
    {
        var result = await workspaces.Commit(
            ParseGuid(request.WorkspaceGuid, nameof(request.WorkspaceGuid)),
            context.CancellationToken);

        var response = new CommitWorkspaceResponse
        {
            CorrelationId = request.CorrelationId,
            WorkspaceGuid = result.WorkspaceGuid.ToString("D")
        };

        response.Commands.AddRange(result.Commands.Select(static command => new CommittedCommandResult
        {
            CommandId = command.CommandId,
            Sequence = command.Sequence,
            EntityGuid = command.EntityGuid?.ToString("D") ?? string.Empty
        }));

        return response;
    }

    public override async Task<Empty> RollbackWorkspace(
        RollbackWorkspaceRequest request,
        ServerCallContext context)
    {
        await workspaces.Rollback(
            ParseGuid(request.WorkspaceGuid, nameof(request.WorkspaceGuid)),
            context.CancellationToken);

        return new Empty();
    }

    private static WorkspaceCommandDraft ToDraft(QueueCommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId))
        {
            throw new ArgumentException("Command id is required.", nameof(request));
        }

        return request.Command.PayloadCase switch
        {
            CommandEnvelope.PayloadOneofCase.ArticleCreate => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.ArticleCreate,
                new { request.Command.ArticleCreate.Name }),
            CommandEnvelope.PayloadOneofCase.ArticleRename => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.ArticleRename,
                new ArticleRenameWorkspaceCommand(
                    ParseGuid(request.Command.ArticleRename.ArticleGuid, nameof(request.Command.ArticleRename.ArticleGuid)),
                    request.Command.ArticleRename.Name)),
            CommandEnvelope.PayloadOneofCase.ItemCreate => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.ItemCreate,
                new
                {
                    ArticleGuid = ParseGuid(request.Command.ItemCreate.ArticleGuid, nameof(request.Command.ItemCreate.ArticleGuid)),
                    ProductionDate = request.Command.ItemCreate.ProductionDate?.ToDateTimeOffset()
                }),
            CommandEnvelope.PayloadOneofCase.ItemMoveToContainer => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.ItemMoveToContainer,
                new ItemMoveToContainerWorkspaceCommand(
                    ParseGuid(request.Command.ItemMoveToContainer.ItemGuid, nameof(request.Command.ItemMoveToContainer.ItemGuid)),
                    request.Command.ItemMoveToContainer.ParentContainerGuid.ToNullableGuid(nameof(request.Command.ItemMoveToContainer.ParentContainerGuid)))),
            CommandEnvelope.PayloadOneofCase.PartitionCreate => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.PartitionCreate,
                new { request.Command.PartitionCreate.Name }),
            CommandEnvelope.PayloadOneofCase.PartitionRename => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.PartitionRename,
                new PartitionRenameWorkspaceCommand(
                    ParseGuid(request.Command.PartitionRename.PartitionGuid, nameof(request.Command.PartitionRename.PartitionGuid)),
                    request.Command.PartitionRename.Name)),
            CommandEnvelope.PayloadOneofCase.PartitionMove => CreateDraft(
                request.CommandId,
                WorkspaceCommandTypes.PartitionMove,
                new PartitionMoveWorkspaceCommand(
                    ParseGuid(request.Command.PartitionMove.PartitionGuid, nameof(request.Command.PartitionMove.PartitionGuid)),
                    ParseGuid(request.Command.PartitionMove.ParentPartitionGuid, nameof(request.Command.PartitionMove.ParentPartitionGuid)))),
            _ => throw new ArgumentException("Command payload is required.", nameof(request))
        };
    }

    private static WorkspaceCommandDraft CreateDraft<TPayload>(
        string commandId,
        string commandType,
        TPayload payload)
        => new(
            commandId,
            commandType,
            1,
            JsonSerializer.Serialize(payload, JsonOptions));

    private static Guid ParseGuid(string value, string fieldName)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new ArgumentException($"Field '{fieldName}' must be a valid GUID.", fieldName);
    }
}

file static class FargoGrpcCommandMappings
{
    public static Guid? ToNullableGuid(this string? value, string fieldName)
        => string.IsNullOrEmpty(value) ? null : ParseNullableGuid(value, fieldName);

    private static Guid ParseNullableGuid(string value, string fieldName)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new ArgumentException($"Field '{fieldName}' must be a valid GUID.", fieldName);
    }
}
