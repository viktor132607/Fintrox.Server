namespace Fintrox.Contracts.Sales;

public sealed record SalesInvoiceLineResponse(
    Guid Id,
    int LineNumber,
    string? ItemCode,
    string Description,
    decimal Quantity,
    string? UnitOfMeasure,
    decimal UnitPrice,
    decimal DiscountPercent,
    Guid VatCodeId,
    string VatCode,
    decimal VatRatePercent,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);
