using Fargo.Core.Actors;
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

    /// <summary>
    /// Adds the name of an article to the metadata.
    /// </summary>
    /// <param name="value">The name value to add.</param>
    public void AddName(Name value)
    {
        Add("name", new AuditValue.String(value));
    }

    /// <summary>
    /// Adds the description of an article to the metadata.
    /// </summary>
    /// <param name="value">The description value to add.</param>
    public void AddDescription(Description value)
    {
        Add("description", new AuditValue.String(value));
    }

    /// <summary>
    /// Adds the shelf life of an article to the metadata.
    /// </summary>
    /// <param name="value">
    /// The shelf life to add, or <see langword="null"/> to record the absence of
    /// a shelf life constraint.
    /// </param>
    public void AddShelfLife(TimeSpan? value)
    {
        Add("shelfLife", value != null
            ? new AuditValue.Number(value.Value.Ticks)
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the color of an article to the metadata.
    /// </summary>
    /// <param name="value">
    /// The color to add, or <see langword="null"/> to record the absence of a
    /// color constraint.
    /// </param>
    public void AddColor(Color? value)
    {
        Add("color", value != null
            ? new AuditValue.Number(value.Value.ToArgb())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the mass of an article to the metadata.
    /// </summary>
    /// <param name="value">
    /// The mass to add, or <see langword="null"/> to record the absence of a mass.
    /// </param>
    public void AddMass(Mass? value)
    {
        Add("mass", value is not null
            ? new AuditValue.String(value.Value.ToString())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the EAN-13 barcode of an article to the metadata.
    /// </summary>
    /// <param name="value">
    /// The EAN-13 barcode to add, or <see langword="null"/> to record the absence
    /// of a barcode.
    /// </param>
    public void AddEan13(Ean13? value)
    {
        Add("ean13", value is not null
            ? new AuditValue.String(value.Value.ToString())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the parent container identifier to the metadata.
    /// </summary>
    /// <param name="parentContainer">
    /// The unique identifier of the parent container, or <see langword="null"/>
    /// when the item is not placed inside a container.
    /// </param>
    public void AddParentContainer(Guid? parentContainer)
    {
        Add("parentContainerGuid", parentContainer is not null
            ? new AuditValue.String(parentContainer.Value.ToString())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the article type to the metadata.
    /// </summary>
    /// <param name="value">The article type to add.</param>
    public void AddArticleType(ArticleType value)
    {
        Add("articleType", new AuditValue.Number((byte)value));
    }

    /// <summary>
    /// Adds the identifier of the source article to the metadata.
    /// </summary>
    /// <param name="fromArticleGuid">
    /// The unique identifier of the article the current article derives from.
    /// </param>
    public void AddFromArticleGuid(Guid fromArticleGuid)
    {
        Add("fromArticle", new AuditValue.String(fromArticleGuid.ToString()));
    }

    /// <summary>
    /// Adds the pack quantity to the metadata.
    /// </summary>
    /// <param name="quantity">
    /// The quantity of the source article represented by the pack.
    /// </param>
    public void AddPackQuantity(Scalar quantity)
    {
        Add("packQuantity", new AuditValue.Number((int)quantity.Amount));
    }

    /// <summary>
    /// Adds the components of a kit article to the metadata.
    /// </summary>
    /// <param name="kitComponents">
    /// The components that compose the kit article.
    /// </param>
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

    /// <summary>
    /// Adds the partitions associated with an entity to the metadata.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the partitions associated with the entity.
    /// </param>
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

    /// <summary>
    /// Adds the partitions that were added to an entity to the metadata.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the partitions that were added.
    /// </param>
    public void AddPartitionsAdded(IReadOnlyCollection<Guid> partitionGuids)
    {
        var values = new List<AuditValue.String>();

        foreach (var p in partitionGuids)
        {
            values.Add(new AuditValue.String(p.ToString()));
        }

        var array = new AuditValue.Array(values);

        Add("partitionsAdded", array);
    }

    /// <summary>
    /// Adds the partitions that were removed from an entity to the metadata.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the partitions that were removed.
    /// </param>
    public void AddPartitionsRemoved(IReadOnlyCollection<Guid> partitionGuids)
    {
        var values = new List<AuditValue.String>();

        foreach (var p in partitionGuids)
        {
            values.Add(new AuditValue.String(p.ToString()));
        }

        var array = new AuditValue.Array(values);

        Add("partitionsRemoved", array);
    }

    /// <summary>
    /// Adds the physical dimensions of an article to the metadata.
    /// </summary>
    /// <param name="x">
    /// The length along the X axis, or <see langword="null"/> when undefined.
    /// </param>
    /// <param name="y">
    /// The length along the Y axis, or <see langword="null"/> when undefined.
    /// </param>
    /// <param name="z">
    /// The length along the Z axis, or <see langword="null"/> when undefined.
    /// </param>
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

    /// <summary>
    /// Adds the unique name identifier of an entity to the metadata.
    /// </summary>
    /// <param name="value">The nameid value to add.</param>
    public void AddNameid(Nameid value)
    {
        Add("nameid", new AuditValue.String(value.ToString()));
    }

    /// <summary>
    /// Adds the first name of a user to the metadata.
    /// </summary>
    /// <param name="value">The first name value to add.</param>
    public void AddFirstName(FirstName value)
    {
        Add("firstName", new AuditValue.String(value.ToString()));
    }

    /// <summary>
    /// Adds the last name of a user to the metadata.
    /// </summary>
    /// <param name="value">The last name value to add.</param>
    public void AddLastName(LastName value)
    {
        Add("lastName", new AuditValue.String(value.ToString()));
    }

    /// <summary>
    /// Adds the active state of an entity to the metadata.
    /// </summary>
    /// <param name="value">
    /// <see langword="true"/> when the entity is active; otherwise,
    /// <see langword="false"/>.
    /// </param>
    public void AddIsActive(bool value)
    {
        Add("isActive", new AuditValue.Boolean(value));
    }

    /// <summary>
    /// Adds the article an item is an instance of to the metadata.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article the item instantiates.
    /// </param>
    public void AddArticleGuid(Guid articleGuid)
    {
        Add("articleGuid", new AuditValue.String(articleGuid.ToString()));
    }

    /// <summary>
    /// Adds the parent partition of a partition to the metadata.
    /// </summary>
    /// <param name="parentPartitionGuid">
    /// The unique identifier of the parent partition, or <see langword="null"/>
    /// when the partition has no parent.
    /// </param>
    public void AddParentPartition(Guid? parentPartitionGuid)
    {
        Add("parentPartitionGuid", parentPartitionGuid is not null
            ? new AuditValue.String(parentPartitionGuid.Value.ToString())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the parent user group of a user group to the metadata.
    /// </summary>
    /// <param name="parentUserGroupGuid">
    /// The unique identifier of the parent user group, or <see langword="null"/>
    /// when the user group has no parent.
    /// </param>
    public void AddParentUserGroup(Guid? parentUserGroupGuid)
    {
        Add("parentUserGroupGuid", parentUserGroupGuid is not null
            ? new AuditValue.String(parentUserGroupGuid.Value.ToString())
            : new AuditValue.Null());
    }

    /// <summary>
    /// Adds the permissions associated with an entity to the metadata.
    /// </summary>
    /// <param name="actions">The actions the entity is permitted to perform.</param>
    public void AddPermissions(IReadOnlyCollection<ActionType> actions)
    {
        Add("permissions", BuildPermissionArray(actions));
    }

    /// <summary>
    /// Adds the permissions that were granted to an entity to the metadata.
    /// </summary>
    /// <param name="actions">The actions that were granted.</param>
    public void AddPermissionsAdded(IReadOnlyCollection<ActionType> actions)
    {
        Add("permissionsAdded", BuildPermissionArray(actions));
    }

    /// <summary>
    /// Adds the permissions that were revoked from an entity to the metadata.
    /// </summary>
    /// <param name="actions">The actions that were revoked.</param>
    public void AddPermissionsRemoved(IReadOnlyCollection<ActionType> actions)
    {
        Add("permissionsRemoved", BuildPermissionArray(actions));
    }

    /// <summary>
    /// Adds the user groups that were added to a user to the metadata.
    /// </summary>
    /// <param name="userGroupGuids">
    /// The unique identifiers of the user groups that were added.
    /// </param>
    public void AddUserGroupsAdded(IReadOnlyCollection<Guid> userGroupGuids)
    {
        Add("userGroupsAdded", BuildGuidArray(userGroupGuids));
    }

    /// <summary>
    /// Adds the user groups that were removed from a user to the metadata.
    /// </summary>
    /// <param name="userGroupGuids">
    /// The unique identifiers of the user groups that were removed.
    /// </param>
    public void AddUserGroupsRemoved(IReadOnlyCollection<Guid> userGroupGuids)
    {
        Add("userGroupsRemoved", BuildGuidArray(userGroupGuids));
    }

    /// <summary>
    /// Adds the partition accesses that were granted to an entity to the metadata.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the partitions the entity gained access to.
    /// </param>
    public void AddPartitionAccessesAdded(IReadOnlyCollection<Guid> partitionGuids)
    {
        Add("partitionAccessesAdded", BuildGuidArray(partitionGuids));
    }

    /// <summary>
    /// Adds the partition accesses that were revoked from an entity to the metadata.
    /// </summary>
    /// <param name="partitionGuids">
    /// The unique identifiers of the partitions the entity lost access to.
    /// </param>
    public void AddPartitionAccessesRemoved(IReadOnlyCollection<Guid> partitionGuids)
    {
        Add("partitionAccessesRemoved", BuildGuidArray(partitionGuids));
    }

    /// <summary>
    /// Records that the authentication password of a user was changed.
    /// </summary>
    /// <remarks>
    /// The password value is never recorded; only the fact that it changed is
    /// captured so the action can be understood without exposing the secret.
    /// </remarks>
    public void AddPasswordChanged()
    {
        Add("passwordChanged", new AuditValue.Boolean(true));
    }

    /// <summary>
    /// Adds the number of refresh tokens that were revoked as a consequence of
    /// the audited action to the metadata.
    /// </summary>
    /// <param name="count">The number of refresh tokens that were revoked.</param>
    public void AddRefreshTokensRevoked(int count)
    {
        Add("refreshTokensRevoked", new AuditValue.Number(count));
    }

    private static AuditValue.Array BuildPermissionArray(IReadOnlyCollection<ActionType> actions)
    {
        var values = new List<AuditValue>();

        foreach (var action in actions)
        {
            values.Add(new AuditValue.Number((int)action));
        }

        return new AuditValue.Array(values);
    }

    private static AuditValue.Array BuildGuidArray(IReadOnlyCollection<Guid> guids)
    {
        var values = new List<AuditValue>();

        foreach (var guid in guids)
        {
            values.Add(new AuditValue.String(guid.ToString()));
        }

        return new AuditValue.Array(values);
    }
}
