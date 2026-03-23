using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Builders;

public class ArticleBuilder
{
    private Name? _name;
    private Description _description = Description.Empty;

    public ArticleBuilder WithName(Name name)
    {
        _name = name;
        return this;
    }

    public ArticleBuilder WithDescription(Description description)
    {
        _description = description;
        return this;
    }

    public Article Build()
    {
        if (_name is null)
        {
            throw new InvalidOperationException("Article Name is required.");
        }

        return new Article
        {
            Name = _name.Value,
            Description = _description
        };
    }
}
