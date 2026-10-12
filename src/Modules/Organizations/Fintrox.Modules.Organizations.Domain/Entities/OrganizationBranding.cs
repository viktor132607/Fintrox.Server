namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationBranding record.</summary>
public sealed class OrganizationBranding : OrganizationEntity
{
    public OrganizationBranding(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid logoDocumentId,
        string primaryColor,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        LogoDocumentId = DomainRules.Id(logoDocumentId, nameof(logoDocumentId));
        PrimaryColor = DomainRules.Text(primaryColor, nameof(primaryColor));
        IsActive = isActive;
    }

    public string Name { get; }
    public Guid LogoDocumentId { get; }
    public string PrimaryColor { get; }
    public bool IsActive { get; }
}
