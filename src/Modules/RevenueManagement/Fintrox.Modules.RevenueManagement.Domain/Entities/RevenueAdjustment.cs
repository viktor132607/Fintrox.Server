namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

public sealed class RevenueAdjustment : OrganizationEntity
{
    public RevenueAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid scheduleLineId,
        decimal amount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        ScheduleLineId = DomainRules.Id(scheduleLineId, nameof(scheduleLineId));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid ScheduleLineId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
