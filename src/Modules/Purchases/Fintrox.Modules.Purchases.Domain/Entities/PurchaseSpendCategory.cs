namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseSpendCategory model. References require application validation.</summary>
public sealed class PurchaseSpendCategory : OrganizationEntity
{
    public PurchaseSpendCategory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid parentCategoryId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        ParentCategoryId = DomainRules.Id(parentCategoryId, nameof(parentCategoryId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public Guid ParentCategoryId { get; }
    public bool IsActive { get; }
}
