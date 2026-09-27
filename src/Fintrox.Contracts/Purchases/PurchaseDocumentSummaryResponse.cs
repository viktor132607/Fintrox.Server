namespace Fintrox.Contracts.Purchases;

public sealed record PurchaseDocumentSummaryResponse(
    Guid Id,
    string? InternalNumber,
    string? SupplierDocumentNumber,
    string Type,
    string Status,
    DateOnly DocumentDate,
    DateOnly DueDate,
    Guid CounterpartyId,
    string SupplierName,
    Guid CurrencyId,
    string CurrencyCode,
    decimal NetTotal,
    decimal VatTotal,
    decimal RecoverableVatTotal,
    decimal NonRecoverableVatTotal,
    decimal GrossTotal,
    DateTimeOffset? ReceivedAtUtc,
    DateTimeOffset? CancelledAtUtc);
