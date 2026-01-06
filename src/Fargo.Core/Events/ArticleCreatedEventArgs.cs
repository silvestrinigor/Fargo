using Fargo.Domain.Entities;

namespace Fargo.Domain.Events
{
    public sealed class ArticleCreatedEventArgs(Article article) : EventArgs
    {
        public Article Article { get; } = article;
    }
}
