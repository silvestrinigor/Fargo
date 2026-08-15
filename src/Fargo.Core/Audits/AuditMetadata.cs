using Fargo.Core.Articles;
using Fargo.Core.Informations;

namespace Fargo.Core.Audits;

/// <summary>
/// Represents metadata associated with an audit record.
/// </summary>
public sealed class AuditMetadata
{
    /// <summary>
    /// Gets the metadata values.
    /// </summary>
    public IReadOnlyDictionary<string, AuditValue> Values => values;

    private readonly Dictionary<string, AuditValue> values = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditMetadata"/> class.
    /// </summary>
    public AuditMetadata() { }

    /// <summary>
    /// Adds a metadata value.
    /// </summary>
    /// <param name="name">The name of the metadata property.</param>
    /// <param name="value">The value of the metadata property.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a metadata property with the specified name already exists.
    /// </exception>
    public void Add(string name, AuditValue value)
    {
        if (!values.TryAdd(name, value))
        {
            throw new InvalidOperationException($"Property '{name}' already existis.");
        }
    }

    public void AddName(Name value)
    {
        Add("name", new AuditValue.String(value));
    }

    public void AddDescription(Description value)
    {
        Add("description", new AuditValue.String(value));
    }

    public void AddArticleType(ArticleType value)
    {
        Add("articleType", new AuditValue.Number((byte)value));
    }
}
