namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ExternalIdentityMapping domain record; references checked in application layer.</summary>
public sealed class ExternalIdentityMapping : OrganizationEntity
{
    public ExternalIdentityMapping(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string externalSubject,
        Guid localPrincipalId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        ExternalSubject = DomainRules.Text(externalSubject, nameof(externalSubject));
        LocalPrincipalId = DomainRules.Id(localPrincipalId, nameof(localPrincipalId));
        IsActive = isActive;
    }

    public Guid ConnectorInstallationId { get; }
    public string ExternalSubject { get; }
    public Guid LocalPrincipalId { get; }
    public bool IsActive { get; }
}
