namespace Fintrox.Contracts.Accounting;

public sealed record AccountingPeriodResponse(
    Guid Id,
    Guid OrganizationId,
    Guid FiscalYearId,
    int Number,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
