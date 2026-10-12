namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationQuota domain record; references checked in application layer.</summary>
public sealed class IntegrationQuota : OrganizationEntity
{
    public IntegrationQuota(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string quotaName,
        long limitPerDay,
        bool isEnforced)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        QuotaName = DomainRules.Text(quotaName, nameof(quotaName));
        LimitPerDay = DomainRules.NonnegativeLong(limitPerDay, nameof(limitPerDay));
        IsEnforced = isEnforced;
    }

    public Guid ConnectorInstallationId { get; }
    public string QuotaName { get; }
    public long LimitPerDay { get; }
    public bool IsEnforced { get; }
}
