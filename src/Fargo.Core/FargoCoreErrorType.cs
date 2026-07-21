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
}
