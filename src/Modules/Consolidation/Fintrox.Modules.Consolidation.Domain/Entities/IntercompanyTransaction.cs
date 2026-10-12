namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned IntercompanyTransaction; external references validated by application layer.</summary>
public sealed class IntercompanyTransaction : OrganizationEntity
{
    public IntercompanyTransaction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceCompanyId,
        Guid targetCompanyId,
        Guid sourceDocumentId,
        DateOnly transactionDate,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SourceCompanyId = DomainRules.Id(sourceCompanyId, nameof(sourceCompanyId));
        TargetCompanyId = DomainRules.Id(targetCompanyId, nameof(targetCompanyId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        TransactionDate = DomainRules.Date(transactionDate, nameof(transactionDate));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SourceCompanyId { get; }
    public Guid TargetCompanyId { get; }
    public Guid SourceDocumentId { get; }
    public DateOnly TransactionDate { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
