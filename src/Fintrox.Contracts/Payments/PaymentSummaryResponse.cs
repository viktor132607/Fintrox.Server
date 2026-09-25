namespace Fintrox.Contracts.Payments;

public sealed record PaymentSummaryResponse(
    Guid Id,
    string? InternalNumber,
    string Direction,
    string Status,
    DateOnly PaymentDate,
    string Method,
    Guid CounterpartyId,
    string CounterpartyName,
    Guid CurrencyId,
    string CurrencyCode,
    decimal Amount,
    decimal AllocatedAmount,
    decimal UnallocatedAmount,
    string? Reference,
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? CancelledAtUtc);
