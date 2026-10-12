namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationReportSnapshot; external references validated by application layer.</summary>
public sealed class ConsolidationReportSnapshot : OrganizationEntity
{
    public ConsolidationReportSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        DateTimeOffset generatedAtUtc,
        Guid documentId,
        int versionNumber)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        GeneratedAtUtc = DomainRules.Instant(generatedAtUtc, nameof(generatedAtUtc));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
    }

    public Guid ConsolidationRunId { get; }
    public DateTimeOffset GeneratedAtUtc { get; }
    public Guid DocumentId { get; }
    public int VersionNumber { get; }
}
