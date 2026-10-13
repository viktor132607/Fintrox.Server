using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Organizations;

public sealed record UpdateOrganizationMemberRoleRequest(
    [param: Required, MaxLength(32)] string Role);
