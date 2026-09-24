namespace Fintrox.Domain.Common;

public abstract class OrganizationScopedAuditableEntity
    : AuditableEntity, IOrganizationScopedEntity
{
    protected OrganizationScopedAuditableEntity()
    {
    }

    protected OrganizationScopedAuditableEntity(
        Guid id,
        Guid organizationId,
        DateTimeOffset now) : base(id, now)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id cannot be empty.",
                nameof(organizationId));
        }

        OrganizationId = organizationId;
    }

    public Guid OrganizationId { get; protected set; }
}
