namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatReturnSubmission; tenant scope and input fields validated at creation.</summary>
public sealed class VatReturnSubmission : OrganizationEntity
{
    public VatReturnSubmission(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid vatReturnId,
        string submissionReference,
        DateTimeOffset submittedAtUtc,
        Guid submittedByUserId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        VatReturnId = DomainRules.Id(vatReturnId, nameof(vatReturnId));
        SubmissionReference = DomainRules.Text(submissionReference, nameof(submissionReference));
        SubmittedAtUtc = DomainRules.Instant(submittedAtUtc, nameof(submittedAtUtc));
        SubmittedByUserId = DomainRules.Id(submittedByUserId, nameof(submittedByUserId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid VatReturnId { get; }
    public string SubmissionReference { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
    public Guid SubmittedByUserId { get; }
    public string Status { get; }
}
