namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmConsentRecord record. Domain invariants enforced at construction.</summary>
public sealed class CrmConsentRecord : OrganizationEntity
{
    public CrmConsentRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string purpose,
        string consentStatus,
        DateTimeOffset recordedAtUtc,
        string source)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Purpose = DomainRules.Text(purpose, nameof(purpose));
        ConsentStatus = DomainRules.Text(consentStatus, nameof(consentStatus));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
        Source = DomainRules.Text(source, nameof(source));
    }

    public Guid CounterpartyId { get; }
    public string Purpose { get; }
    public string ConsentStatus { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string Source { get; }
}
