namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityContactRole record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityContactRole : OrganizationEntity
{
    public OpportunityContactRole(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        Guid contactId,
        string role,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        ContactId = DomainRules.Id(contactId, nameof(contactId));
        Role = DomainRules.Text(role, nameof(role));
        IsPrimary = isPrimary;
    }

    public Guid OpportunityId { get; }
    public Guid ContactId { get; }
    public string Role { get; }
    public bool IsPrimary { get; }
}
