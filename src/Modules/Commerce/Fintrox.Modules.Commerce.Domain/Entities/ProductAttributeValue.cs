namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ProductAttributeValue; external references validated by application layer.</summary>
public sealed class ProductAttributeValue : OrganizationEntity
{
    public ProductAttributeValue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid variantId,
        Guid attributeDefinitionId,
        string value,
        int sortOrder)
        : base(id, organizationId, createdAtUtc)
    {
        VariantId = DomainRules.Id(variantId, nameof(variantId));
        AttributeDefinitionId = DomainRules.Id(attributeDefinitionId, nameof(attributeDefinitionId));
        Value = DomainRules.Text(value, nameof(value));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
    }

    public Guid VariantId { get; }
    public Guid AttributeDefinitionId { get; }
    public string Value { get; }
    public int SortOrder { get; }
}
