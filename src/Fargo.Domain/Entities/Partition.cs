using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Partition
    {
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

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

        public IReadOnlyCollection<User> UsersWithAccess => usersWithAccess;

        private readonly List<User> usersWithAccess = [];

        public void AddUserAccess(User user)
        {
            if(usersWithAccess.Contains(user))
                return;

            usersWithAccess.Add(user);
        }

        public void RemoveUserAccess(User user)
        {
            if(!usersWithAccess.Contains(user))
                return;

            usersWithAccess.Remove(user);
        }

        public IReadOnlyCollection<Article> Articles => articles;

        private readonly List<Article> articles = [];

        public void AddArticle(Article article)
        {
            if(articles.Contains(article))
                return;

            articles.Add(article);
        }

        public void RemoveArticle(Article article)
        {
            if(!articles.Contains(article))
                return;

            articles.Remove(article);
        }

        public IReadOnlyCollection<Item> Items => items;

        private readonly List<Item> items = [];

        public void AddItem(Item item)
        {
            if(items.Contains(item))
                return;

            items.Add(item);
        }

        public void RemoveItem(Item item)
        {
            if(!items.Contains(item))
                return;

            items.Remove(item);
        }

        public IReadOnlyCollection<User> Users => users;

        private readonly List<User> users = [];

        public void AddUser(User user)
        {
            if(users.Contains(user))
                return;

            users.Add(user);
        }

        public void RemoveUser(User user)
        {
            if(!users.Contains(user))
                return;

            users.Remove(user);
        }
    }
}