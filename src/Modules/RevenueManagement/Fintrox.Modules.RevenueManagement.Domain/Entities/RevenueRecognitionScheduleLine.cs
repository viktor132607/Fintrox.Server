namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionScheduleLine model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionScheduleLine : OrganizationEntity
{
    public RevenueRecognitionScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid scheduleId,
        Guid performanceObligationId,
        DateOnly recognizeOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ScheduleId = DomainRules.Id(scheduleId, nameof(scheduleId));
        PerformanceObligationId = DomainRules.Id(performanceObligationId, nameof(performanceObligationId));
        RecognizeOn = DomainRules.Date(recognizeOn, nameof(recognizeOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ScheduleId { get; }
    public Guid PerformanceObligationId { get; }
    public DateOnly RecognizeOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
