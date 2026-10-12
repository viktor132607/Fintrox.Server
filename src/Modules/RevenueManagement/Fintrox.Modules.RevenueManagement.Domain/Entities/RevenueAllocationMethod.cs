namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueAllocationMethod model; no cross-module navigations.</summary>
public sealed class RevenueAllocationMethod : OrganizationEntity
{
    public RevenueAllocationMethod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string methodCode,
        string description,
        string allocationBasis,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        MethodCode = DomainRules.Text(methodCode, nameof(methodCode));
        Description = DomainRules.Text(description, nameof(description));
        AllocationBasis = DomainRules.Text(allocationBasis, nameof(allocationBasis));
        IsActive = isActive;
    }

    public string MethodCode { get; }
    public string Description { get; }
    public string AllocationBasis { get; }
    public bool IsActive { get; }
}
