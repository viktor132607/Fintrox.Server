namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped FiscalYearConfiguration domain record; referenced objects are validated by application use cases.</summary>
public sealed class FiscalYearConfiguration : OrganizationEntity
{
    public FiscalYearConfiguration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fiscalYearId,
        int startMonth,
        string calendarType,
        bool isAdjustmentPeriodAllowed)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalYearId = DomainRules.Id(fiscalYearId, nameof(fiscalYearId));
        StartMonth = DomainRules.PositiveInt(startMonth, nameof(startMonth));
        CalendarType = DomainRules.Text(calendarType, nameof(calendarType));
        IsAdjustmentPeriodAllowed = isAdjustmentPeriodAllowed;
    }

    public Guid FiscalYearId { get; }
    public int StartMonth { get; }
    public string CalendarType { get; }
    public bool IsAdjustmentPeriodAllowed { get; }
}
