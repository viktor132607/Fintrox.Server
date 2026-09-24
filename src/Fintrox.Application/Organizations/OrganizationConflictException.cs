namespace Fintrox.Application.Organizations;

public sealed class OrganizationConflictException(string message) : Exception(message);
