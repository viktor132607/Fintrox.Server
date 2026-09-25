namespace Fintrox.Contracts.Sales;

public sealed record SalesInvoiceSummaryResponse(
    Guid Id,
    string? Number,
    string Status,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    Guid CounterpartyId,
    string CustomerName,
    Guid CurrencyId,
    string CurrencyCode,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    DateTimeOffset? IssuedAtUtc,
    DateTimeOffset? CancelledAtUtc);
