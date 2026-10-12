namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseRequestBudgetCheck model. References require application validation.</summary>
public sealed class PurchaseRequestBudgetCheck : OrganizationEntity
{
    public PurchaseRequestBudgetCheck(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseRequisitionId,
        Guid budgetLineId,
        decimal requestedAmount,
        string currency,
        DateTimeOffset checkedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseRequisitionId = DomainRules.Id(purchaseRequisitionId, nameof(purchaseRequisitionId));
        BudgetLineId = DomainRules.Id(budgetLineId, nameof(budgetLineId));
        RequestedAmount = DomainRules.Nonnegative(requestedAmount, nameof(requestedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        CheckedAtUtc = DomainRules.Instant(checkedAtUtc, nameof(checkedAtUtc));
    }

    public Guid PurchaseRequisitionId { get; }
    public Guid BudgetLineId { get; }
    public decimal RequestedAmount { get; }
    public string Currency { get; }
    public DateTimeOffset CheckedAtUtc { get; }
}
