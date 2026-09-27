namespace Fintrox.Contracts.Payments;

public sealed record PaymentAllocationResponse(
    Guid Id,
    int LineNumber,
    string TargetType,
    Guid TargetDocumentId,
    string DocumentNumber,
    Guid DocumentCurrencyId,
    string DocumentCurrencyCode,
    decimal DocumentExchangeRate,
    decimal DocumentAmount,
    decimal PaymentAmount);
