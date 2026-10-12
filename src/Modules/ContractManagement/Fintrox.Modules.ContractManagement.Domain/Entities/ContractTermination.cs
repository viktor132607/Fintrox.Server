namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractTermination; external references validated by application layer.</summary>
public sealed class ContractTermination : OrganizationEntity
{
    public ContractTermination(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        DateOnly effectiveOn,
        string terminationReason,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        TerminationReason = DomainRules.Text(terminationReason, nameof(terminationReason));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid BusinessContractId { get; }
    public DateOnly EffectiveOn { get; }
    public string TerminationReason { get; }
    public Guid ApprovedByUserId { get; }
}
