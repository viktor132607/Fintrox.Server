namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxDeterminationRule; tenant scope and input fields validated at creation.</summary>
public sealed class TaxDeterminationRule : OrganizationEntity
{
    public TaxDeterminationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jurisdictionId,
        Guid taxCategoryId,
        string transactionType,
        int priority,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        TaxCategoryId = DomainRules.Id(taxCategoryId, nameof(taxCategoryId));
        TransactionType = DomainRules.Text(transactionType, nameof(transactionType));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
        IsActive = isActive;
    }

    public Guid JurisdictionId { get; }
    public Guid TaxCategoryId { get; }
    public string TransactionType { get; }
    public int Priority { get; }
    public bool IsActive { get; }
}
