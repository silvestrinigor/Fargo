using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    ///
    /// A user contains authentication credentials and a collection
    /// of permissions that define which actions they are allowed to perform.
    /// </summary>
    public class User : Entity
    {
        /// <summary>
        /// Gets or sets the unique NAMEID (username) of the user.
        /// This value uniquely identifies the user in the system.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the textual description associated with the user.
        /// If not specified, the description defaults to <see cref="Description.Empty"/>.
        /// </summary>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets or sets the hashed password of the user.
        ///
        /// The raw password is never stored. Instead, a secure hash
        /// is persisted using the application's password hashing strategy.
        /// </summary>
        public required PasswordHash PasswordHash
        {
            get;
            set;
        }

        /// <summary>
        /// The default number of days a user can keep the same password
        /// before a password change is required.
        /// </summary>
        public const int DefaultPasswordChangeDays = 90;

        /// <summary>
        /// Gets or sets the default password expiration period for the user.
        ///
        /// This value is persisted and represents the amount of time added to the
        /// current UTC time when the user successfully changes their own password.
        ///
        /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
        /// immediately after being changed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the assigned value is less than <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public TimeSpan DefaultPasswordExpirationTimeSpan
        {
            get;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
                field = value;
            }
        } = TimeSpan.FromDays(DefaultPasswordChangeDays);

        /// <summary>
        /// Gets or sets the date and time when the user must change their password.
        ///
        /// By default, this value is initialized based on
        /// <see cref="DefaultPasswordChangeDays"/> from the current UTC time.
        /// </summary>
        public DateTimeOffset RequirePasswordChangeAt
        {
            get;
            set;
        } = DateTimeOffset.UtcNow + TimeSpan.FromDays(DefaultPasswordChangeDays);

        /// <summary>
        /// Resets the password expiration date based on the user's
        /// <see cref="DefaultPasswordExpirationTimeSpan"/>.
        ///
        /// The new expiration date is calculated by adding the configured
        /// default expiration interval to the current UTC time.
        ///
        /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
        /// immediately.
        /// </summary>
        /// <remarks>
        /// This method is typically used after the user successfully changes
        /// their own password.
        /// </remarks>
        public void ResetPasswordExpiration()
            => RequirePasswordChangeAt = DateTimeOffset.UtcNow + DefaultPasswordExpirationTimeSpan;

        /// <summary>
        /// Sets the password expiration requirement to a future date based on the specified number of days.
        /// </summary>
        /// <param name="days">
        /// The number of days from the current UTC time after which the user must change their password.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="days"/> is less than zero.
        /// </exception>
        public void RequirePasswordChangeInDays(int days)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(days);

            RequirePasswordChangeAt = DateTimeOffset.UtcNow.AddDays(days);
        }

        /// <summary>
        /// Marks the user's password as requiring an immediate change.
        /// </summary>
        /// <remarks>
        /// After calling this method, <see cref="IsPasswordChangeRequired"/> will return <c>true</c>
        /// until the password is updated and a new expiration date is set.
        /// </remarks>
        public void MarkPasswordChangeAsRequired()
        {
            RequirePasswordChangeAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Determines whether the user is currently required to change their password.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current UTC time is greater than or equal to
        /// <see cref="RequirePasswordChangeAt"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPasswordChangeRequired
            => DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

        /// <summary>
        /// Gets the read-only collection of permissions assigned to the user.
        ///
        /// Each permission represents an allowed <see cref="ActionType"/>
        /// that the user can perform.
        /// </summary>
        public IReadOnlyCollection<UserPermission> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store user permissions.
        /// </summary>
        private readonly List<UserPermission> userPermissions = [];

        /// <summary>
        /// Adds a permission to the user if it does not already exist.
        /// </summary>
        /// <param name="action">The action type to allow.</param>
        public void AddPermission(ActionType action)
        {
            if (userPermissions.Any(x => x.Action == action))
            {
                return;
            }

            var userPermission = new UserPermission
            {
                Action = action,
                User = this
            };

            userPermissions.Add(userPermission);
        }

        /// <summary>
        /// Removes a permission from the user if it exists.
        /// </summary>
        /// <param name="action">The action type to remove.</param>
        public void RemovePermission(ActionType action)
        {
            var userPermission = userPermissions.SingleOrDefault(x => x.Action == action);

            if (userPermission == null)
            {
                return;
            }

            userPermissions.Remove(userPermission);
        }

        /// <summary>
        /// Determines whether the user has the specified permission.
        /// </summary>
        /// <param name="action">The action type to check.</param>
        /// <returns>
        /// <c>true</c> if the user has the permission; otherwise <c>false</c>.
        /// </returns>
        public bool HasPermission(ActionType action)
            => UserPermissions.Any(p => p.Action == action);

        /// <summary>
        /// Validates whether the user has the specified permission.
        /// </summary>
        /// <param name="action">The action that must be authorized.</param>
        /// <exception cref="UserNotAuthorizedFargoDomainException">
        /// Thrown when the user does not have the required permission.
        /// </exception>
        public void ValidatePermission(ActionType action)
        {
            if (!HasPermission(action))
            {
                throw new UserNotAuthorizedFargoDomainException(Guid, action);
            }
        }
    }
}