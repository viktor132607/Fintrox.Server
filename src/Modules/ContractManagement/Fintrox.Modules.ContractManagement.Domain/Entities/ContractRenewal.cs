namespace Fintrox.Modules.ContractManagement.Domain.Entities;

public sealed class ContractRenewal : OrganizationEntity
{
    public ContractRenewal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractId,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid ContractId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
