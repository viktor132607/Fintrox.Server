namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentTemplateVariable. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentTemplateVariable : OrganizationEntity
{
    public DocumentTemplateVariable(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid documentTemplateId,
        string variableCode,
        string dataType,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        DocumentTemplateId = DomainRules.Id(documentTemplateId, nameof(documentTemplateId));
        VariableCode = DomainRules.Text(variableCode, nameof(variableCode));
        DataType = DomainRules.Text(dataType, nameof(dataType));
        IsRequired = isRequired;
    }

    public Guid DocumentTemplateId { get; }
    public string VariableCode { get; }
    public string DataType { get; }
    public bool IsRequired { get; }
}
