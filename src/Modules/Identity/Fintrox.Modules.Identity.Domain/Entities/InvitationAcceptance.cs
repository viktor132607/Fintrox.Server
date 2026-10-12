namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned InvitationAcceptance domain record; references checked in application layer.</summary>
public sealed class InvitationAcceptance : OrganizationEntity
{
    public InvitationAcceptance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid organizationInvitationId,
        Guid acceptedByUserId,
        DateTimeOffset acceptedAtUtc,
        string acceptanceMethod)
        : base(id, organizationId, createdAtUtc)
    {
        OrganizationInvitationId = DomainRules.Id(organizationInvitationId, nameof(organizationInvitationId));
        AcceptedByUserId = DomainRules.Id(acceptedByUserId, nameof(acceptedByUserId));
        AcceptedAtUtc = DomainRules.Instant(acceptedAtUtc, nameof(acceptedAtUtc));
        AcceptanceMethod = DomainRules.Text(acceptanceMethod, nameof(acceptanceMethod));
    }

    public Guid OrganizationInvitationId { get; }
    public Guid AcceptedByUserId { get; }
    public DateTimeOffset AcceptedAtUtc { get; }
    public string AcceptanceMethod { get; }
}
