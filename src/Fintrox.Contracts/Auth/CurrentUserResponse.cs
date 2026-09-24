namespace Fintrox.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    IReadOnlyList<UserOrganizationResponse> Organizations);
