namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankReconciliationAdjustment record; cross-module references require application authorization.</summary>
public sealed class BankReconciliationAdjustment : OrganizationEntity
{
    public BankReconciliationAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reconciliationSessionId,
        Guid statementLineId,
        string adjustmentType,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ReconciliationSessionId = DomainRules.Id(reconciliationSessionId, nameof(reconciliationSessionId));
        StatementLineId = DomainRules.Id(statementLineId, nameof(statementLineId));
        AdjustmentType = DomainRules.Text(adjustmentType, nameof(adjustmentType));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ReconciliationSessionId { get; }
    public Guid StatementLineId { get; }
    public string AdjustmentType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
