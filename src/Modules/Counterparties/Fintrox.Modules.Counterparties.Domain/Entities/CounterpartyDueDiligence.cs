namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyDueDiligence. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyDueDiligence : OrganizationEntity
{
    public CounterpartyDueDiligence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string reviewType,
        DateOnly reviewedOn,
        DateOnly expiresOn,
        string outcome)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        ReviewType = DomainRules.Text(reviewType, nameof(reviewType));
        ReviewedOn = DomainRules.Date(reviewedOn, nameof(reviewedOn));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
        Outcome = DomainRules.Text(outcome, nameof(outcome));
    }

    public Guid CounterpartyId { get; }
    public string ReviewType { get; }
    public DateOnly ReviewedOn { get; }
    public DateOnly ExpiresOn { get; }
    public string Outcome { get; }
}
