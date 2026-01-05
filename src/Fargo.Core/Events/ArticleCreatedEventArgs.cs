using Fargo.Domain.Entities;

namespace Fargo.Domain.Events
{
    public sealed class ArticleCreatedEventArgs(Article article) : EventArgs
    {
        public Guid ArticleGuid { get; } = article.Guid;
        public Article Article { get; } = article;
    }
}
