namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractTemplate; external references validated by application layer.</summary>
public sealed class ContractTemplate : OrganizationEntity
{
    public ContractTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string templateCode,
        string name,
        string contractType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        TemplateCode = DomainRules.Text(templateCode, nameof(templateCode));
        Name = DomainRules.Text(name, nameof(name));
        ContractType = DomainRules.Text(contractType, nameof(contractType));
        IsActive = isActive;
    }

    public string TemplateCode { get; }
    public string Name { get; }
    public string ContractType { get; }
    public bool IsActive { get; }
}
