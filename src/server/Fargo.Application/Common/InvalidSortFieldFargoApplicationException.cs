using Fargo.Core.Common;

namespace Fargo.Application.Common;

/// <summary>
/// Thrown when a list query requests a sort field that is not in the aggregate's
/// allowed set. Surfaced as 400 Bad Request. Enumerating the allowed fields in the
/// message helps the caller correct the request and avoids any attempt to sort by
/// an arbitrary or untranslatable column.
/// </summary>
public sealed class InvalidSortFieldFargoApplicationException(string field, IEnumerable<string> allowedFields)
    : FargoApplicationException(
        $"Cannot sort by '{field}'. Allowed sort fields: {string.Join(", ", allowedFields)}.",
        FargoErrorType.InvalidOperation);