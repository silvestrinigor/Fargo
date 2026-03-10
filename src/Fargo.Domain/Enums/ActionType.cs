namespace Fargo.Domain.Enums
{
    /// <summary>
    /// Represents the set of actions that can be authorized in the system.
    ///
    /// Each value defines a specific permission that can be granted
    /// to a user through <see cref="Entities.UserPermission"/>.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Allows creating new articles in the system.
        /// </summary>
        CreateArticle,

        /// <summary>
        /// Allows deleting existing articles from the system.
        /// </summary>
        DeleteArticle,

        /// <summary>
        /// Allows editing existing articles from the system.
        /// </summary>
        EditArticle,

        /// <summary>
        /// Allows creating new items associated with articles.
        /// </summary>
        CreateItem,

        /// <summary>
        /// Allows deleting items from the system.
        /// </summary>
        DeleteItem,

        /// <summary>
        /// Allows editing items from the system.
        /// </summary>
        EditItem,

        /// <summary>
        /// Allows creating new users.
        /// </summary>
        CreateUser,

        /// <summary>
        /// Allows deleting users.
        /// </summary>
        DeleteUser,

        /// <summary>
        /// Allows editing users.
        /// </summary>
        EditUser,

        /// <summary>
        /// Allows changing the password of another user without requiring
        /// the current password of that user.
        ///
        /// This permission is typically intended for administrative users
        /// who need to reset another user's password. It does not apply to
        /// a user changing their own password, which should require the
        /// current password.
        /// </summary>
        ChangeOtherUserPassword,

        /// <summary>
        /// Allows creating partitions in the system.
        /// </summary>
        CreatePartition,

        /// <summary>
        /// Allows deleting partitions.
        /// </summary>
        DeletePartition,

        /// <summary>
        /// Allows editing partitions.
        /// </summary>
        EditPartition,
    }
}