namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmCustomerHealthScore record. Domain invariants enforced at construction.</summary>
public sealed class CrmCustomerHealthScore : OrganizationEntity
{
    public CrmCustomerHealthScore(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        int score,
        DateTimeOffset calculatedAtUtc,
        string healthBand)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Score = DomainRules.NonnegativeInt(score, nameof(score));
        CalculatedAtUtc = DomainRules.Instant(calculatedAtUtc, nameof(calculatedAtUtc));
        HealthBand = DomainRules.Text(healthBand, nameof(healthBand));
    }

    public Guid CounterpartyId { get; }
    public int Score { get; }
    public DateTimeOffset CalculatedAtUtc { get; }
    public string HealthBand { get; }
}
