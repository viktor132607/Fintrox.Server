using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Organizations;

public sealed record UpdateOrganizationMemberRoleRequest(
    [property: Required, MaxLength(32)] string Role);
