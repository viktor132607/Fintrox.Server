namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionSchedule model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionSchedule : OrganizationEntity
{
    public RevenueRecognitionSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        Guid accountingBookId,
        DateOnly startsOn,
        DateOnly endsOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid RevenueContractId { get; }
    public Guid AccountingBookId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Currency { get; }
}
