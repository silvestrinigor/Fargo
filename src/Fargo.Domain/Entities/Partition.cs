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

        private readonly HashSet<Article> articles = [];

        public IReadOnlyCollection<Article> Articles => articles;

        public bool AddArticle(Article article)
        {
            return articles.Add(article);
        }

        public bool RemoveArticle(Article article)
        {
            return articles.Remove(article);
        }

        public readonly HashSet<User> userAccesses = [];

        public IReadOnlyCollection<User> UserAccesses => userAccesses;

        public bool AddUserAccess(User user)
        {
            return userAccesses.Add(user);
        }

        public bool RemoveUserAccess(User user)
        {
            return userAccesses.Remove(user);
        }

        private readonly HashSet<Partition> partitions = [];

        public IReadOnlyCollection<Partition> Partitions => partitions;
    }
}