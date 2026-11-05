using Fargo.Application.Common;
using Fargo.Application.Operations;
using Fargo.Application.Operations.Articles;
using Fargo.Core.Audits;

namespace Fargo.Application.Tests.Common;

public class AuditLogTypeResolverTests
{
    [Theory]
    [InlineData(typeof(ArticleCreateCommand), AuditLogType.EntityCreated)]
    [InlineData(typeof(ArticleUpdateCommand), AuditLogType.EntityUpdated)]
    [InlineData(typeof(ArticleDeleteCommand), AuditLogType.EntityDeleted)]
    public void Resolve_WhenMappedCommand_ReturnsExpectedType(Type commandType, AuditLogType expected)
    {
        Assert.Equal(expected, AuditLogTypeResolver.Resolve(commandType));
    }

    [Fact]
    public void Resolve_WhenUnmappedType_Throws()
    {
        Assert.Throws<NotSupportedException>(() => AuditLogTypeResolver.Resolve(typeof(UnmappedCommand)));
    }

    [Fact]
    public void Resolve_EveryCommandInApplication_HasAMapping()
    {
        var commandInterface = typeof(ICommand);

        var commandTypes = typeof(ArticleCreateCommand).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                && commandInterface.IsAssignableFrom(t));

        foreach (var commandType in commandTypes)
        {
            // Throws NotSupportedException if any command is missing a mapping.
            _ = AuditLogTypeResolver.Resolve(commandType);
        }
    }

    private sealed record UnmappedCommand : ICommand;
}
