using System.Linq.Expressions;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Mappings
{
    public static class ItemMappings
    {
        public static readonly Expression<Func<Item, ItemInformation>> InformationProjection =
            i => new ItemInformation(
                i.Guid,
                i.ArticleGuid
            );

        public static ItemInformation ToInformation(this Item i) =>
            new(
                i.Guid,
                i.ArticleGuid
            );
    }
}