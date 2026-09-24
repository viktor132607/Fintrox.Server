namespace Fintrox.Application.Organizations;

public sealed class OrganizationMembershipConflictException(string message)
    : Exception(message);
