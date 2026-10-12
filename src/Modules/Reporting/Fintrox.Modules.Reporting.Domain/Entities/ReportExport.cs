namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportExport model. References require application validation.</summary>
public sealed class ReportExport : OrganizationEntity
{
    public ReportExport(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportRunId,
        string exportFormat,
        Guid documentId,
        DateTimeOffset exportedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ReportRunId = DomainRules.Id(reportRunId, nameof(reportRunId));
        ExportFormat = DomainRules.Text(exportFormat, nameof(exportFormat));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        ExportedAtUtc = DomainRules.Instant(exportedAtUtc, nameof(exportedAtUtc));
    }

    public Guid ReportRunId { get; }
    public string ExportFormat { get; }
    public Guid DocumentId { get; }
    public DateTimeOffset ExportedAtUtc { get; }
}
