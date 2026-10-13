using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Organizations;

public sealed record AddOrganizationMemberRequest(
    [param: Required, EmailAddress, MaxLength(256)] string Email,
    [param: Required, MaxLength(32)] string Role);
