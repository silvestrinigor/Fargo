using Fargo.GrpcContracts.Commands.V1;

namespace Fargo.GrpcApi.Tests;

public sealed class WorkspaceContractTests
{
    [Fact]
    public void QueueCommandRequest_ShouldExposeWorkspaceCommandEnvelope()
    {
        var request = new QueueCommandRequest
        {
            WorkspaceGuid = Guid.NewGuid().ToString("D"),
            CommandId = "cmd-1",
            Command = new CommandEnvelope
            {
                ArticleCreate = new ArticleCreateCommand
                {
                    Name = "Article"
                }
            }
        };

        Assert.Equal(CommandEnvelope.PayloadOneofCase.ArticleCreate, request.Command.PayloadCase);
        Assert.Equal("cmd-1", request.CommandId);
    }
}
