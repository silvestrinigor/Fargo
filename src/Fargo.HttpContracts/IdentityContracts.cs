namespace Fargo.HttpContracts;

public sealed record LoginRequest(
    string Nameid,
    string Password
);

public sealed record RefreshRequest(
    string RefreshToken
);

public sealed record PasswordUpdateRequest(
    string NewPassword,
    string CurrentPassword
);

public sealed record AuthDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
