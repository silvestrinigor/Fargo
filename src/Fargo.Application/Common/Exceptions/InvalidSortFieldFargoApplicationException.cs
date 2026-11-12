using Fargo.Core.Common;

namespace Fargo.Application.Common.Exceptions;

/// <summary>
/// A list query sorted by a field outside the allowed set. Surfaced as 400 Bad Request.
/// </summary>
public sealed class InvalidSortFieldFargoApplicationException(string field, IEnumerable<string> allowedFields)
    : FargoApplicationException(
        $"Cannot sort by '{field}'. Allowed sort fields: {string.Join(", ", allowedFields)}.",
        FargoErrorType.InvalidOperation);
