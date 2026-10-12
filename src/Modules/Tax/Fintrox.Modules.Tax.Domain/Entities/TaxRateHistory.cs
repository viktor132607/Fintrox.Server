namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxRateHistory; tenant scope and input fields validated at creation.</summary>
public sealed class TaxRateHistory : OrganizationEntity
{
    public TaxRateHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid taxRateId,
        decimal previousPercent,
        decimal newPercent,
        DateOnly changedOn,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        TaxRateId = DomainRules.Id(taxRateId, nameof(taxRateId));
        PreviousPercent = DomainRules.Percent(previousPercent, nameof(previousPercent));
        NewPercent = DomainRules.Percent(newPercent, nameof(newPercent));
        ChangedOn = DomainRules.Date(changedOn, nameof(changedOn));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid TaxRateId { get; }
    public decimal PreviousPercent { get; }
    public decimal NewPercent { get; }
    public DateOnly ChangedOn { get; }
    public string Reason { get; }
}
