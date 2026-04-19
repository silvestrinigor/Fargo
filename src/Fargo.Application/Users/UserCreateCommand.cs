using Fargo.Application.Partitions;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Command used to create a new <see cref="User"/>.
/// </summary>
/// <param name="User">
/// The data required to create the user, including identity, credentials,
/// optional permissions, and initial partition assignment.
/// </param>
/// <remarks>
/// This command represents the intention to create a new user within a specific
/// authorization and partition context.
/// </remarks>
public sealed record UserCreateCommand(
        UserCreateModel User
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="UserCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Resolving and authorizing the current actor</description></item>
/// <item><description>Validating permission to create users</description></item>
/// <item><description>Validating access to the target partition</description></item>
/// <item><description>Hashing the user password securely</description></item>
/// <item><description>Applying domain validation rules via <see cref="UserService"/></description></item>
/// <item><description>Assigning permissions and partition access</description></item>
/// <item><description>Persisting the new user</description></item>
/// </list>
///
/// Partition behavior:
/// <list type="bullet">
/// <item><description>
/// If <c>FirstPartition</c> is not provided, the user is assigned to the global partition
/// </description></item>
/// <item><description>
/// The actor must have access to the selected partition
/// </description></item>
/// </list>
///
/// Security considerations:
/// <list type="bullet">
/// <item><description>The password is hashed using <see cref="IPasswordHasher"/> before persistence</description></item>
/// <item><description>The user is required to change the password after creation</description></item>
/// <item><description>Permission assignment is explicitly controlled during creation</description></item>
/// </list>
/// </remarks>
public sealed class UserCreateCommandHandler(
        ActorService actorService,
        UserService userService,
        IUserRepository userRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<UserCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new user.
    /// </summary>
    /// <param name="command">The command containing the user creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The unique identifier of the created user.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or is not authorized.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified or resolved partition does not exist.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create items.
    /// </exception>
    /// <remarks>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>Resolve the current actor</description></item>
    /// <item><description>Validate <see cref="ActionType.CreateUser"/> permission</description></item>
    /// <item><description>Resolve the target partition (or fallback to global)</description></item>
    /// <item><description>Validate partition access</description></item>
    /// <item><description>Hash the user password</description></item>
    /// <item><description>Create the user entity</description></item>
    /// <item><description>Apply password policies (expiration and required change)</description></item>
    /// <item><description>Validate domain rules via <see cref="UserService"/></description></item>
    /// <item><description>Assign permissions</description></item>
    /// <item><description>Persist the user</description></item>
    /// </list>
    /// </remarks>
    public async Task<Guid> Handle(
            UserCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateUser);

        var partitionGuid = command.User.FirstPartition ?? PartitionService.GlobalPartitionGuid;

        var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        var nameid = ValidateNameid(command.User.Nameid);

        ValidatePasswordPolicy(command.User.Password);

        var userPasswordHash = passwordHasher.Hash(command.User.Password);

        var user = new User
        {
            Nameid = nameid,
            FirstName = command.User.FirstName,
            LastName = command.User.LastName,
            Description = command.User.Description ?? Description.Empty,
            PasswordHash = userPasswordHash
        };

        user.Partitions.Add(partition);

        if (command.User.DefaultPasswordExpirationTimeSpan is not null)
        {
            user.DefaultPasswordExpirationPeriod =
                command.User.DefaultPasswordExpirationTimeSpan.Value;
        }

        user.MarkPasswordChangeAsRequired();

        await userService.ValidateUserCreate(user, cancellationToken);

        foreach (var permission in command.User.Permissions ?? [])
        {
            user.AddPermission(permission.Action);
        }

        userRepository.Add(user);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishUserCreated(user.Guid, user.Nameid, [partition.Guid], cancellationToken);

        return user.Guid;
    }

    private static Nameid ValidateNameid(string value)
    {
        try
        {
            return new Nameid(value);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }
    }

    private static void ValidatePasswordPolicy(string password)
    {
        try
        {
            _ = new Password(password);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}
