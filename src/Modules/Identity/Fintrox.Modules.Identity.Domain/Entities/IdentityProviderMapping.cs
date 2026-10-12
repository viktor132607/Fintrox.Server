namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned IdentityProviderMapping domain record; references checked in application layer.</summary>
public sealed class IdentityProviderMapping : OrganizationEntity
{
    public IdentityProviderMapping(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string providerCode,
        string externalSubject,
        Guid userId,
        DateTimeOffset linkedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        ExternalSubject = DomainRules.Text(externalSubject, nameof(externalSubject));
        UserId = DomainRules.Id(userId, nameof(userId));
        LinkedAtUtc = DomainRules.Instant(linkedAtUtc, nameof(linkedAtUtc));
    }

    public string ProviderCode { get; }
    public string ExternalSubject { get; }
    public Guid UserId { get; }
    public DateTimeOffset LinkedAtUtc { get; }
}
