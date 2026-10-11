namespace Fintrox.Modules.ContractManagement.Domain.Entities;

public sealed class BusinessContract : OrganizationEntity
{
    public BusinessContract(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string reference,
        ContractKind kind,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Reference = DomainRules.Text(reference, nameof(reference));
        Kind = DomainRules.Defined(kind, nameof(kind));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid CounterpartyId { get; }
    public string Reference { get; }
    public ContractKind Kind { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
