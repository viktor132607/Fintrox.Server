namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationAdjustment; external references validated by application layer.</summary>
public sealed class ConsolidationAdjustment : OrganizationEntity
{
    public ConsolidationAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        string adjustmentCode,
        string description,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        AdjustmentCode = DomainRules.Text(adjustmentCode, nameof(adjustmentCode));
        Description = DomainRules.Text(description, nameof(description));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ConsolidationRunId { get; }
    public string AdjustmentCode { get; }
    public string Description { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
