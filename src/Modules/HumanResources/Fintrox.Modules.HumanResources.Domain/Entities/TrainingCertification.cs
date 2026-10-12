namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned TrainingCertification domain model.</summary>
public sealed class TrainingCertification : OrganizationEntity
{
    public TrainingCertification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string certificationCode,
        DateOnly issuedOn,
        DateOnly expiresOn,
        string issuer)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        CertificationCode = DomainRules.Text(certificationCode, nameof(certificationCode));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
        Issuer = DomainRules.Text(issuer, nameof(issuer));
    }

    public Guid EmployeeId { get; }
    public string CertificationCode { get; }
    public DateOnly IssuedOn { get; }
    public DateOnly ExpiresOn { get; }
    public string Issuer { get; }
}
