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
        /// Allows creating new items associated with articles.
        /// </summary>
        CreateItem,

        /// <summary>
        /// Allows deleting items from the system.
        /// </summary>
        DeleteItem,

        /// <summary>
        /// Allows creating new users.
        /// </summary>
        CreateUser,

        /// <summary>
        /// Allows deleting users.
        /// </summary>
        DeleteUser,

        /// <summary>
        /// Allows creating partitions in the system.
        /// </summary>
        CreatePartition,

        /// <summary>
        /// Allows deleting partitions.
        /// </summary>
        DeletePartition
    }
}