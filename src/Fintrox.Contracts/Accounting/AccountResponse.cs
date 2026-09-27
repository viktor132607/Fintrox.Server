namespace Fintrox.Contracts.Accounting;

public sealed record AccountResponse(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Type,
    Guid? ParentAccountId,
    bool IsAnalytical,
    bool IsActive,
    bool AllowManualPosting,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
