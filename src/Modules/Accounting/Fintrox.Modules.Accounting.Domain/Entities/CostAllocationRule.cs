namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped CostAllocationRule domain record; referenced objects are validated by application use cases.</summary>
public sealed class CostAllocationRule : OrganizationEntity
{
    public CostAllocationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        Guid sourceAccountId,
        DateOnly effectiveFrom,
        string allocationBasis,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        SourceAccountId = DomainRules.Id(sourceAccountId, nameof(sourceAccountId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        AllocationBasis = DomainRules.Text(allocationBasis, nameof(allocationBasis));
        IsActive = isActive;
    }

    public string Code { get; }
    public Guid SourceAccountId { get; }
    public DateOnly EffectiveFrom { get; }
    public string AllocationBasis { get; }
    public bool IsActive { get; }
}
