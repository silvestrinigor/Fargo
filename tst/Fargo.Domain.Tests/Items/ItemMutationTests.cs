using Fargo.Core.Articles;
using Fargo.Core.Items;

namespace Fargo.Core.Tests.Items;

public sealed class ItemMutationTests
{
    [Fact]
    public void Constructor_Should_SetItemActiveByDefault()
    {
        var item = new Item(Article.CreateArticle(new Name("Article")));

        Assert.True(item.IsActive);
    }

    [Fact]
    public void ActivateDeactivate_Should_UpdateActiveState()
    {
        var item = new Item(Article.CreateArticle(new Name("Article")));

        item.Deactivate();
        Assert.False(item.IsActive);

        item.Activate();
        Assert.True(item.IsActive);
    }
}
