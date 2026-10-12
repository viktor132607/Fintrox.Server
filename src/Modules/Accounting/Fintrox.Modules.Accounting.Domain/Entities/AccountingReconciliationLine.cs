namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountingReconciliationLine domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountingReconciliationLine : OrganizationEntity
{
    public AccountingReconciliationLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reconciliationId,
        Guid referenceId,
        string referenceType,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ReconciliationId = DomainRules.Id(reconciliationId, nameof(reconciliationId));
        ReferenceId = DomainRules.Id(referenceId, nameof(referenceId));
        ReferenceType = DomainRules.Text(referenceType, nameof(referenceType));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ReconciliationId { get; }
    public Guid ReferenceId { get; }
    public string ReferenceType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
