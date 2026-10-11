namespace Fintrox.Modules.ServiceManagement.Domain;

/// <summary>Module-owned identity and organization scope. References are validated by application use cases.</summary>
public abstract class OrganizationEntity
{
    protected OrganizationEntity(Guid id, Guid organizationId, DateTimeOffset createdAtUtc)
    {
        Id = DomainRules.Id(id, nameof(id));
        OrganizationId = DomainRules.Id(organizationId, nameof(organizationId));
        CreatedAtUtc = DomainRules.Instant(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; }
    public Guid OrganizationId { get; }
    public DateTimeOffset CreatedAtUtc { get; }
}
