using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Models
{
    public class Article : Model
    {
        internal Article() { }

        public override ModelType ModelType => ModelType.Article;

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
    }
}
