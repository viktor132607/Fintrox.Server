namespace Fintrox.Modules.Identity.Domain.Entities;

public sealed class ServiceAccount : OrganizationEntity
{
    public ServiceAccount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid ownerUserId)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
    }

    public string Name { get; }
    public Guid OwnerUserId { get; }
}
