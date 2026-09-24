namespace Fintrox.Contracts.Auth;

public sealed record UserOrganizationResponse(
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationSlug,
    string Role,
    IReadOnlyCollection<string> Permissions);
