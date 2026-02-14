namespace LostUAL.Contracts.Auth;
public sealed record AccountProfileDto(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc
);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record ChangeEmailRequest(string CurrentPassword, string NewEmail);
