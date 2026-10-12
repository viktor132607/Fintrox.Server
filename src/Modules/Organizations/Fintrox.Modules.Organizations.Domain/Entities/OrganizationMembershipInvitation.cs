namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationMembershipInvitation record.</summary>
public sealed class OrganizationMembershipInvitation : OrganizationEntity
{
    public OrganizationMembershipInvitation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string invitedEmail,
        Guid invitedByUserId,
        DateTimeOffset expiresAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        InvitedEmail = DomainRules.Text(invitedEmail, nameof(invitedEmail));
        InvitedByUserId = DomainRules.Id(invitedByUserId, nameof(invitedByUserId));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public string InvitedEmail { get; }
    public Guid InvitedByUserId { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public string Status { get; }
}
