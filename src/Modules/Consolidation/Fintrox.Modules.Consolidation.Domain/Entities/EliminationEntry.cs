namespace Fintrox.Modules.Consolidation.Domain.Entities;

public sealed class EliminationEntry : OrganizationEntity
{
    public EliminationEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationPeriodId,
        Guid sourceOrganizationId,
        Guid targetOrganizationId,
        Guid sourceJournalId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationPeriodId = DomainRules.Id(consolidationPeriodId, nameof(consolidationPeriodId));
        SourceOrganizationId = DomainRules.Id(sourceOrganizationId, nameof(sourceOrganizationId));
        TargetOrganizationId = DomainRules.Id(targetOrganizationId, nameof(targetOrganizationId));
        SourceJournalId = DomainRules.Id(sourceJournalId, nameof(sourceJournalId));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (SourceOrganizationId == TargetOrganizationId) throw new ArgumentException("Eliminations require two companies.", nameof(targetOrganizationId));
    }

    public Guid ConsolidationPeriodId { get; }
    public Guid SourceOrganizationId { get; }
    public Guid TargetOrganizationId { get; }
    public Guid SourceJournalId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
