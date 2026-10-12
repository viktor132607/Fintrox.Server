namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractComplianceRequirement; external references validated by application layer.</summary>
public sealed class ContractComplianceRequirement : OrganizationEntity
{
    public ContractComplianceRequirement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        string requirementCode,
        string description,
        int reviewFrequencyDays)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        RequirementCode = DomainRules.Text(requirementCode, nameof(requirementCode));
        Description = DomainRules.Text(description, nameof(description));
        ReviewFrequencyDays = DomainRules.PositiveInt(reviewFrequencyDays, nameof(reviewFrequencyDays));
    }

    public Guid BusinessContractId { get; }
    public string RequirementCode { get; }
    public string Description { get; }
    public int ReviewFrequencyDays { get; }
}
