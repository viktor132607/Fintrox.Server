namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractAmendment; external references validated by application layer.</summary>
public sealed class ContractAmendment : OrganizationEntity
{
    public ContractAmendment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        string amendmentNumber,
        DateOnly effectiveOn,
        string reason,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        AmendmentNumber = DomainRules.Text(amendmentNumber, nameof(amendmentNumber));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid BusinessContractId { get; }
    public string AmendmentNumber { get; }
    public DateOnly EffectiveOn { get; }
    public string Reason { get; }
    public string Status { get; }
}
