namespace Fintrox.Modules.ContractManagement.Domain.Entities;

public sealed class ContractVersion : OrganizationEntity
{
    public ContractVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractId,
        int versionNumber,
        string terms)
        : base(id, organizationId, createdAtUtc)
    {
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        Terms = DomainRules.Text(terms, nameof(terms));
    }

    public Guid ContractId { get; }
    public int VersionNumber { get; }
    public string Terms { get; }
}
