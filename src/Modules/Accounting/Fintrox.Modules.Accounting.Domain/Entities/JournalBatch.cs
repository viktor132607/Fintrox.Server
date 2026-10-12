namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped JournalBatch domain record; referenced objects are validated by application use cases.</summary>
public sealed class JournalBatch : OrganizationEntity
{
    public JournalBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string batchNumber,
        Guid accountingBookId,
        DateOnly documentDate,
        string description,
        string status,
        Guid createdByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        BatchNumber = DomainRules.Text(batchNumber, nameof(batchNumber));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        DocumentDate = DomainRules.Date(documentDate, nameof(documentDate));
        Description = DomainRules.Text(description, nameof(description));
        Status = DomainRules.Text(status, nameof(status));
        CreatedByUserId = DomainRules.Id(createdByUserId, nameof(createdByUserId));
    }

    public string BatchNumber { get; }
    public Guid AccountingBookId { get; }
    public DateOnly DocumentDate { get; }
    public string Description { get; }
    public string Status { get; }
    public Guid CreatedByUserId { get; }
}
