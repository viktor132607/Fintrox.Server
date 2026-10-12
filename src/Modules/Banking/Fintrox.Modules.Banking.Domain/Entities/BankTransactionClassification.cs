namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankTransactionClassification record; cross-module references require application authorization.</summary>
public sealed class BankTransactionClassification : OrganizationEntity
{
    public BankTransactionClassification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid statementLineId,
        Guid categoryId,
        decimal confidencePercent,
        DateTimeOffset classifiedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StatementLineId = DomainRules.Id(statementLineId, nameof(statementLineId));
        CategoryId = DomainRules.Id(categoryId, nameof(categoryId));
        ConfidencePercent = DomainRules.Percent(confidencePercent, nameof(confidencePercent));
        ClassifiedAtUtc = DomainRules.Instant(classifiedAtUtc, nameof(classifiedAtUtc));
    }

    public Guid StatementLineId { get; }
    public Guid CategoryId { get; }
    public decimal ConfidencePercent { get; }
    public DateTimeOffset ClassifiedAtUtc { get; }
}
