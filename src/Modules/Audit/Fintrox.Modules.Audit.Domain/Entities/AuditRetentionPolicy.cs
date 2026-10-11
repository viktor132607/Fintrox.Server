namespace Fintrox.Modules.Audit.Domain.Entities;

public sealed class AuditRetentionPolicy : OrganizationEntity
{
    public AuditRetentionPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        int retentionDays)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        RetentionDays = DomainRules.PositiveInt(retentionDays, nameof(retentionDays));
    }

    public string Name { get; }
    public int RetentionDays { get; }
}
