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
    /// The main administrator user cannot be deleted.
    /// </summary>
    CannotDeleteMainAdminUser = 1,

    /// <summary>
    /// The unique nameid is already in use.
    /// </summary>
    UserNameidAlrealdyInUse = 2,

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
    PartitionCircularHierarchy = 9
}
