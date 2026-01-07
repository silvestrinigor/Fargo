using Fargo.Domain.Entities.Models;

namespace Fargo.Domain.Events
{
    public sealed class ArticleCreatedEventArgs(Article article) : EventArgs
    {
        public Article Article { get; } = article;
    }
}
