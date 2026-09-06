using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;

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

    public void AddShelfLife(TimeSpan? value)
    {
        Add("shelfLife", value != null
            ? new AuditValue.Number(value.Value.Ticks)
            : new AuditValue.Null());
    }

    public void AddColor(Color? value)
    {
        Add("color", value != null
            ? new AuditValue.Number(value.Value.ToArgb())
            : new AuditValue.Null());
    }

    public void AddMass(Mass? value)
    {
        Add("mass", value is not null
            ? new AuditValue.String(value.Value.ToString())
            : new AuditValue.Null());
    }

    public void AddEan13(Ean13? value)
    {
        Add("ean13", value is not null
            ? new AuditValue.String(value.Value.ToString())
            : new AuditValue.Null());
    }

    public void AddArticleType(ArticleType value)
    {
        Add("articleType", new AuditValue.Number((byte)value));
    }

    public void AddFromArticleGuid(Guid fromArticleGuid)
    {
        Add("fromArticle", new AuditValue.String(fromArticleGuid.ToString()));
    }

    public void AddPackQuantity(Scalar quantity)
    {
        Add("packQuantity", new AuditValue.Number((int)quantity.Amount));
    }

    public void AddKitComponents(IReadOnlyCollection<ArticleKitComponentInformation> kitComponents)
    {
        var values = new List<AuditValue.Object>();

        foreach (var k in kitComponents)
        {
            var obj = new Dictionary<string, AuditValue>
            {
                { "fromArticle", new AuditValue.String(k.FromArticleGuid.ToString()) },
                { "quantity", new AuditValue.Number((int)k.Quantity.Amount) }
            };

            var auditObj = new AuditValue.Object(obj);

            values.Add(auditObj);
        }

        var array = new AuditValue.Array(values);

        Add("kitComponents", array);
    }

    public void AddPartitions(IReadOnlyCollection<Guid> partitionGuids)
    {
        var values = new List<AuditValue.String>();

        foreach (var p in partitionGuids)
        {
            values.Add(new AuditValue.String(p.ToString()));
        }

        var array = new AuditValue.Array(values);

        Add("partitions", array);
    }

    public void AddDimension(Length? x, Length? y, Length? z)
    {
        var obj = new Dictionary<string, AuditValue>
            {
                {
                    "x",
                    x is not null ? new AuditValue.String(x.Value.ToString()) : new AuditValue.Null()
                },
                {
                    "y",
                    y is not null ? new AuditValue.String(y.Value.ToString()) : new AuditValue.Null()
                },
                {
                    "z",
                    z is not null ? new AuditValue.String(z.Value.ToString()) : new AuditValue.Null()
                },
            };

        var auditObj = new AuditValue.Object(obj);

        Add("dimension", auditObj);
    }
}
