namespace Fintrox.Contracts.Tax;

public sealed record VatCodeResponse(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Kind,
    decimal RatePercent,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool AppliesToSales,
    bool AppliesToPurchases,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
