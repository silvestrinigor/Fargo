using Fargo.Core.Common;

namespace Fargo.Application.Common.Exceptions;

/// <summary>
/// The idempotency key was already used for a different operation, so replaying its stored result would answer
/// the wrong question.
/// </summary>
public sealed class IdempotencyKeyReusedFargoApplicationException(Guid key, string commandName)
    : FargoApplicationException(
        $"The idempotency key '{key}' was already used for a different operation ({commandName}). Use a new key for a new request.",
        FargoErrorType.InvalidOperation)
{
    public Guid Key { get; } = key;
}