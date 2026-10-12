namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractParty; external references validated by application layer.</summary>
public sealed class ContractParty : OrganizationEntity
{
    public ContractParty(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        Guid counterpartyId,
        string role,
        string signatoryName,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Role = DomainRules.Text(role, nameof(role));
        SignatoryName = DomainRules.Text(signatoryName, nameof(signatoryName));
        IsPrimary = isPrimary;
    }

    public Guid BusinessContractId { get; }
    public Guid CounterpartyId { get; }
    public string Role { get; }
    public string SignatoryName { get; }
    public bool IsPrimary { get; }
}
