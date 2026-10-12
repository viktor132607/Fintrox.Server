namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned ElectronicInvoiceSubmission; tenant scope and input fields validated at creation.</summary>
public sealed class ElectronicInvoiceSubmission : OrganizationEntity
{
    public ElectronicInvoiceSubmission(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceInvoiceId,
        string submissionReference,
        DateTimeOffset submittedAtUtc,
        string providerCode,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SourceInvoiceId = DomainRules.Id(sourceInvoiceId, nameof(sourceInvoiceId));
        SubmissionReference = DomainRules.Text(submissionReference, nameof(submissionReference));
        SubmittedAtUtc = DomainRules.Instant(submittedAtUtc, nameof(submittedAtUtc));
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SourceInvoiceId { get; }
    public string SubmissionReference { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
    public string ProviderCode { get; }
    public string Status { get; }
}
