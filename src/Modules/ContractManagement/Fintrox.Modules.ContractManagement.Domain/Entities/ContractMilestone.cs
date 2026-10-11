namespace Fintrox.Modules.ContractManagement.Domain.Entities;

public sealed class ContractMilestone : OrganizationEntity
{
    public ContractMilestone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractId,
        string name,
        DateOnly dueOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        Name = DomainRules.Text(name, nameof(name));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ContractId { get; }
    public string Name { get; }
    public DateOnly DueOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
