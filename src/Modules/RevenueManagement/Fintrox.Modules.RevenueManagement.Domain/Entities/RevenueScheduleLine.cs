namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

public sealed class RevenueScheduleLine : OrganizationEntity
{
    public RevenueScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid performanceObligationId,
        Guid recognitionRuleId,
        DateOnly recognitionDate,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PerformanceObligationId = DomainRules.Id(performanceObligationId, nameof(performanceObligationId));
        RecognitionRuleId = DomainRules.Id(recognitionRuleId, nameof(recognitionRuleId));
        RecognitionDate = DomainRules.Date(recognitionDate, nameof(recognitionDate));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PerformanceObligationId { get; }
    public Guid RecognitionRuleId { get; }
    public DateOnly RecognitionDate { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
