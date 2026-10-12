namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAttributeDefinition domain record; references checked in application layer.</summary>
public sealed class InventoryAttributeDefinition : OrganizationEntity
{
    public InventoryAttributeDefinition(
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
