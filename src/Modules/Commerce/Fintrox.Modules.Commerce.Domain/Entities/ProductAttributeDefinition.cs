namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ProductAttributeDefinition; external references validated by application layer.</summary>
public sealed class ProductAttributeDefinition : OrganizationEntity
{
    public ProductAttributeDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string dataType,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        DataType = DomainRules.Text(dataType, nameof(dataType));
        IsRequired = isRequired;
    }

    public string Code { get; }
    public string Name { get; }
    public string DataType { get; }
    public bool IsRequired { get; }
}
