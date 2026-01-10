using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Partition
    {
        public required Name Name
        {
            get;
            set;
        }

        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public IReadOnlyCollection<Article> Articles => articles;

        private readonly HashSet<Article> articles = [];

        public bool AddArticle(Article article)
        {
            return articles.Add(article);
        }

        public bool RemoveArticle(Article article)
        {
            return articles.Remove(article);
        }
    }
}
