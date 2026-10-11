namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class JobApplication : OrganizationEntity
{
    public JobApplication(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jobOpeningId,
        string applicantName,
        string email)
        : base(id, organizationId, createdAtUtc)
    {
        JobOpeningId = DomainRules.Id(jobOpeningId, nameof(jobOpeningId));
        ApplicantName = DomainRules.Text(applicantName, nameof(applicantName));
        Email = DomainRules.Text(email, nameof(email));
    }

    public Guid JobOpeningId { get; }
    public string ApplicantName { get; }
    public string Email { get; }
}
