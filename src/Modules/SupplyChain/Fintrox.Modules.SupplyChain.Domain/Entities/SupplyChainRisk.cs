namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyChainRisk domain record. No cross-module navigation properties.</summary>
public sealed class SupplyChainRisk : OrganizationEntity
{
    public SupplyChainRisk(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string riskCode,
        Guid supplierId,
        string impact,
        string likelihood,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        RiskCode = DomainRules.Text(riskCode, nameof(riskCode));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        Impact = DomainRules.Text(impact, nameof(impact));
        Likelihood = DomainRules.Text(likelihood, nameof(likelihood));
        IsActive = isActive;
    }

    public string RiskCode { get; }
    public Guid SupplierId { get; }
    public string Impact { get; }
    public string Likelihood { get; }
    public bool IsActive { get; }
}
