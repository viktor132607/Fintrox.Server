namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractApproval; external references validated by application layer.</summary>
public sealed class ContractApproval : OrganizationEntity
{
    public ContractApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractVersionId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ContractVersionId = DomainRules.Id(contractVersionId, nameof(contractVersionId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid ContractVersionId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
