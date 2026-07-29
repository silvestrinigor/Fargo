namespace Fargo.Core;

/// <summary>
/// 
/// </summary>
public enum FargoCoreErrorType
{
    /// <summary>
    /// Not defined.
    /// </summary>
    None = 0,

    /// <summary>
    /// 
    /// </summary>
    ArticleBarcodeAlreadyInUse = 3,

    /// <summary>
    /// 
    /// </summary>
    CannotDeleteArticleThatIsDependencyOfAnotherArticle = 4,

    /// <summary>
    /// 
    /// </summary>
    CannotDeleteArticleWithItemsAssociated = 5,

    /// <summary>
    /// 
    /// </summary>
    CannotDeleteMainAdministratorsUserGroup = 6,

    /// <summary>
    /// 
    /// </summary>
    UserGroupNameidAlrealdyInUse = 7,

    /// <summary>
    /// 
    /// </summary>
    CannotDeleteGlobalPartition = 8,

    /// <summary>
    /// 
    /// </summary>
    PartitionCircularHierarchy = 9,

    /// <summary>
    /// 
    /// </summary>
    PartitionCannotBeOwnParentPartition = 10,

    /// <summary>
    /// 
    /// </summary>
    ItemCannotBeOwnContainer = 11,

    /// <summary>
    /// 
    /// </summary>
    ItemIsNotContainer = 12,

    /// <summary>
    /// 
    /// </summary>
    ItemCircularContainerHierarchy = 13,

    /// <summary>
    /// 
    /// </summary>
    GlobalPartitionCannotBePartOfAnotherPartition = 14,

    /// <summary>
    /// 
    /// </summary>
    CannotDeletePartitionWithEntitiesAssociated = 15,
}
