using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Events
{
    public sealed class ArticleDeletedEventArgs(Article article) : EventArgs
    {
        public Article Article { get; } = article;
    }
}
