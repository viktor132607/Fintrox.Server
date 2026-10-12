namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationContact record.</summary>
public sealed class OrganizationContact : OrganizationEntity
{
    public OrganizationContact(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid legalEntityId,
        string contactType,
        string contactValue,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        LegalEntityId = DomainRules.Id(legalEntityId, nameof(legalEntityId));
        ContactType = DomainRules.Text(contactType, nameof(contactType));
        ContactValue = DomainRules.Text(contactValue, nameof(contactValue));
        IsPrimary = isPrimary;
    }

    public Guid LegalEntityId { get; }
    public string ContactType { get; }
    public string ContactValue { get; }
    public bool IsPrimary { get; }
}
