namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentExportJob. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentExportJob : OrganizationEntity
{
    public DocumentExportJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid requestedByUserId,
        string exportFormat,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        ExportFormat = DomainRules.Text(exportFormat, nameof(exportFormat));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid RequestedByUserId { get; }
    public string ExportFormat { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
