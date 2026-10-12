namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportResultSnapshot model. References require application validation.</summary>
public sealed class ReportResultSnapshot : OrganizationEntity
{
    public ReportResultSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportRunId,
        long rowCount,
        DateTimeOffset generatedAtUtc,
        string storageReference)
        : base(id, organizationId, createdAtUtc)
    {
        ReportRunId = DomainRules.Id(reportRunId, nameof(reportRunId));
        RowCount = DomainRules.NonnegativeLong(rowCount, nameof(rowCount));
        GeneratedAtUtc = DomainRules.Instant(generatedAtUtc, nameof(generatedAtUtc));
        StorageReference = DomainRules.Text(storageReference, nameof(storageReference));
    }

    public Guid ReportRunId { get; }
    public long RowCount { get; }
    public DateTimeOffset GeneratedAtUtc { get; }
    public string StorageReference { get; }
}
