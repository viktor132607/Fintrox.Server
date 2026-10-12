namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCertification model; no cross-module navigations.</summary>
public sealed class ResourceCertification : OrganizationEntity
{
    public ResourceCertification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        string certificationCode,
        DateOnly issuedOn,
        DateOnly expiresOn,
        string issuer)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        CertificationCode = DomainRules.Text(certificationCode, nameof(certificationCode));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
        Issuer = DomainRules.Text(issuer, nameof(issuer));
    }

    public Guid ResourceId { get; }
    public string CertificationCode { get; }
    public DateOnly IssuedOn { get; }
    public DateOnly ExpiresOn { get; }
    public string Issuer { get; }
}
