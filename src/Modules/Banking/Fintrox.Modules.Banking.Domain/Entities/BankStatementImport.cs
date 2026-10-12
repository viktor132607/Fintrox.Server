namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankStatementImport record; cross-module references require application authorization.</summary>
public sealed class BankStatementImport : OrganizationEntity
{
    public BankStatementImport(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        Guid sourceDocumentId,
        Guid importedByUserId,
        DateTimeOffset importedAtUtc,
        string importFormat)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        ImportedByUserId = DomainRules.Id(importedByUserId, nameof(importedByUserId));
        ImportedAtUtc = DomainRules.Instant(importedAtUtc, nameof(importedAtUtc));
        ImportFormat = DomainRules.Text(importFormat, nameof(importFormat));
    }

    public Guid BankAccountId { get; }
    public Guid SourceDocumentId { get; }
    public Guid ImportedByUserId { get; }
    public DateTimeOffset ImportedAtUtc { get; }
    public string ImportFormat { get; }
}
