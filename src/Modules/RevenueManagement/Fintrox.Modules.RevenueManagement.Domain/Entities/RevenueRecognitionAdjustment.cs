namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionAdjustment model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionAdjustment : OrganizationEntity
{
    public RevenueRecognitionAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid recognitionEventId,
        string reason,
        decimal adjustmentAmount,
        string currency,
        DateOnly adjustedOn)
        : base(id, organizationId, createdAtUtc)
    {
        RecognitionEventId = DomainRules.Id(recognitionEventId, nameof(recognitionEventId));
        Reason = DomainRules.Text(reason, nameof(reason));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AdjustedOn = DomainRules.Date(adjustedOn, nameof(adjustedOn));
    }

    public Guid RecognitionEventId { get; }
    public string Reason { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public DateOnly AdjustedOn { get; }
}
