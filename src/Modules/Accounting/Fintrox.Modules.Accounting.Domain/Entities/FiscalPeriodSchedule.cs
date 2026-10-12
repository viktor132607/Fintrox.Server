namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped FiscalPeriodSchedule domain record; referenced objects are validated by application use cases.</summary>
public sealed class FiscalPeriodSchedule : OrganizationEntity
{
    public FiscalPeriodSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fiscalYearId,
        int periodNumber,
        DateOnly startsOn,
        DateOnly endsOn,
        bool isAdjustment)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalYearId = DomainRules.Id(fiscalYearId, nameof(fiscalYearId));
        PeriodNumber = DomainRules.PositiveInt(periodNumber, nameof(periodNumber));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IsAdjustment = isAdjustment;
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid FiscalYearId { get; }
    public int PeriodNumber { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public bool IsAdjustment { get; }
}
