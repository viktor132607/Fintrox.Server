namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionPolicy model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionPolicy : OrganizationEntity
{
    public RevenueRecognitionPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string policyCode,
        string name,
        string method,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PolicyCode = DomainRules.Text(policyCode, nameof(policyCode));
        Name = DomainRules.Text(name, nameof(name));
        Method = DomainRules.Text(method, nameof(method));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public string PolicyCode { get; }
    public string Name { get; }
    public string Method { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
