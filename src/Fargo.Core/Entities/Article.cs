using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article
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

        public DateTime CreatedAt 
        { 
            get; 
            init; 
        } = DateTime.Now;
    }
}
