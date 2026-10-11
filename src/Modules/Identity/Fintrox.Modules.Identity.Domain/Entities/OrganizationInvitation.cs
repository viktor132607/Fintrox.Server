namespace Fintrox.Modules.Identity.Domain.Entities;

public sealed class OrganizationInvitation : OrganizationEntity
{
    public OrganizationInvitation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string email,
        Guid invitedByUserId,
        DateTimeOffset expiresAtUtc,
        string tokenHash)
        : base(id, organizationId, createdAtUtc)
    {
        Email = DomainRules.Text(email, nameof(email));
        InvitedByUserId = DomainRules.Id(invitedByUserId, nameof(invitedByUserId));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        TokenHash = DomainRules.Hash(tokenHash, nameof(tokenHash));
        if (ExpiresAtUtc <= CreatedAtUtc) throw new ArgumentException("Invitation must expire after creation.", nameof(expiresAtUtc));
    }

    public string Email { get; }
    public Guid InvitedByUserId { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public string TokenHash { get; }
}
