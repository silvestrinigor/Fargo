using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data required to update a user's password.
    /// </summary>
    /// <remarks>
    /// The required fields depend on who is performing the operation:
    ///
    /// <list type="bullet">
    /// <item>
    /// When a user changes their own password, the <see cref="CurrentPassword"/>
    /// must be provided to verify the request.
    /// </item>
    /// <item>
    /// When an administrator resets the password of another user,
    /// the <see cref="CurrentPassword"/> is not required.
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="NewPassword">
    /// The new password that will replace the current one.
    /// </param>
    /// <param name="CurrentPassword">
    /// The user's current password used to verify the request when the user
    /// is changing their own password. This value may be <see langword="null"/>
    /// when an administrator resets the password of another user.
    /// </param>
    public sealed record UserPasswordUpdateModel(
            Password NewPassword,
            Password? CurrentPassword = null
            );
}