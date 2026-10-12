namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxExemptionCertificate; tenant scope and input fields validated at creation.</summary>
public sealed class TaxExemptionCertificate : OrganizationEntity
{
    public TaxExemptionCertificate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string certificateNumber,
        DateOnly startsOn,
        DateOnly endsOn,
        Guid documentId)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        CertificateNumber = DomainRules.Text(certificateNumber, nameof(certificateNumber));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid CounterpartyId { get; }
    public string CertificateNumber { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public Guid DocumentId { get; }
}
