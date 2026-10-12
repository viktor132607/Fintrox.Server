namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentOcrResult. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentOcrResult : OrganizationEntity
{
    public DocumentOcrResult(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        string languageCode,
        string textContent,
        decimal confidencePercent,
        DateTimeOffset processedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        LanguageCode = DomainRules.Text(languageCode, nameof(languageCode));
        TextContent = DomainRules.Text(textContent, nameof(textContent));
        ConfidencePercent = DomainRules.Percent(confidencePercent, nameof(confidencePercent));
        ProcessedAtUtc = DomainRules.Instant(processedAtUtc, nameof(processedAtUtc));
    }

    public Guid StoredDocumentId { get; }
    public string LanguageCode { get; }
    public string TextContent { get; }
    public decimal ConfidencePercent { get; }
    public DateTimeOffset ProcessedAtUtc { get; }
}
