namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped OpeningBalanceImport domain record; referenced objects are validated by application use cases.</summary>
public sealed class OpeningBalanceImport : OrganizationEntity
{
    public OpeningBalanceImport(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fiscalYearId,
        Guid accountingBookId,
        Guid sourceDocumentId,
        DateTimeOffset importedAtUtc,
        Guid importedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalYearId = DomainRules.Id(fiscalYearId, nameof(fiscalYearId));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        ImportedAtUtc = DomainRules.Instant(importedAtUtc, nameof(importedAtUtc));
        ImportedByUserId = DomainRules.Id(importedByUserId, nameof(importedByUserId));
    }

    public Guid FiscalYearId { get; }
    public Guid AccountingBookId { get; }
    public Guid SourceDocumentId { get; }
    public DateTimeOffset ImportedAtUtc { get; }
    public Guid ImportedByUserId { get; }
}
