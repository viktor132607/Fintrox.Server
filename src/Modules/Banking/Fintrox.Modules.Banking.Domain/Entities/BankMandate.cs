namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankMandate record; cross-module references require application authorization.</summary>
public sealed class BankMandate : OrganizationEntity
{
    public BankMandate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string mandateReference,
        DateOnly signedOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        MandateReference = DomainRules.Text(mandateReference, nameof(mandateReference));
        SignedOn = DomainRules.Date(signedOn, nameof(signedOn));
        IsActive = isActive;
    }

    public Guid CounterpartyId { get; }
    public string MandateReference { get; }
    public DateOnly SignedOn { get; }
    public bool IsActive { get; }
}
