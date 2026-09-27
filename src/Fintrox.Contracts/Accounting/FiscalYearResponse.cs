namespace Fintrox.Contracts.Accounting;

public sealed record FiscalYearResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
