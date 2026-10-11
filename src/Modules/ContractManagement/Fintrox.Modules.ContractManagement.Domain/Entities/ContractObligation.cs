namespace Fintrox.Modules.ContractManagement.Domain.Entities;

public sealed class ContractObligation : OrganizationEntity
{
    public ContractObligation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractId,
        string description,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        Description = DomainRules.Text(description, nameof(description));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
    }

    public Guid ContractId { get; }
    public string Description { get; }
    public DateOnly DueOn { get; }
}
