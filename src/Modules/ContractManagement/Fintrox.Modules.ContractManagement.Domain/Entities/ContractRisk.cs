namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractRisk; external references validated by application layer.</summary>
public sealed class ContractRisk : OrganizationEntity
{
    public ContractRisk(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        string riskCode,
        string impact,
        string likelihood,
        string mitigation)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        RiskCode = DomainRules.Text(riskCode, nameof(riskCode));
        Impact = DomainRules.Text(impact, nameof(impact));
        Likelihood = DomainRules.Text(likelihood, nameof(likelihood));
        Mitigation = DomainRules.Text(mitigation, nameof(mitigation));
    }

    public Guid BusinessContractId { get; }
    public string RiskCode { get; }
    public string Impact { get; }
    public string Likelihood { get; }
    public string Mitigation { get; }
}
