namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentMetadataField. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentMetadataField : OrganizationEntity
{
    public DocumentMetadataField(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string displayName,
        string fieldType,
        bool isSearchable)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        DisplayName = DomainRules.Text(displayName, nameof(displayName));
        FieldType = DomainRules.Text(fieldType, nameof(fieldType));
        IsSearchable = isSearchable;
    }

    public string Code { get; }
    public string DisplayName { get; }
    public string FieldType { get; }
    public bool IsSearchable { get; }
}
