namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxGroupMember; tenant scope and input fields validated at creation.</summary>
public sealed class TaxGroupMember : OrganizationEntity
{
    public TaxGroupMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid taxGroupId,
        Guid taxCategoryId,
        DateOnly validFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        TaxGroupId = DomainRules.Id(taxGroupId, nameof(taxGroupId));
        TaxCategoryId = DomainRules.Id(taxCategoryId, nameof(taxCategoryId));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        IsActive = isActive;
    }

    public Guid TaxGroupId { get; }
    public Guid TaxCategoryId { get; }
    public DateOnly ValidFrom { get; }
    public bool IsActive { get; }
}
