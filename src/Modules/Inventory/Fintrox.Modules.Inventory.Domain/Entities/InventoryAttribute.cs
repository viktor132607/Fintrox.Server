namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAttribute domain record; references checked in application layer.</summary>
public sealed class InventoryAttribute : OrganizationEntity
{
    public InventoryAttribute(
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
