namespace Fintrox.Contracts.Organizations;

public sealed record OrganizationMemberResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
