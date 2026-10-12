namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractTemplateVersion; external references validated by application layer.</summary>
public sealed class ContractTemplateVersion : OrganizationEntity
{
    public ContractTemplateVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractTemplateId,
        int versionNumber,
        string templateJson,
        DateOnly publishedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ContractTemplateId = DomainRules.Id(contractTemplateId, nameof(contractTemplateId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        TemplateJson = DomainRules.Json(templateJson, nameof(templateJson));
        PublishedOn = DomainRules.Date(publishedOn, nameof(publishedOn));
    }

    public Guid ContractTemplateId { get; }
    public int VersionNumber { get; }
    public string TemplateJson { get; }
    public DateOnly PublishedOn { get; }
}
