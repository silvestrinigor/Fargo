using Fargo.Domain.Entities;

namespace Fargo.Domain.Events
{
    public sealed class ArticleDeletedEventArgs(Article article) : EventArgs
    {
        public Guid ArticleGuid { get; } = article.Guid;
    }
}
