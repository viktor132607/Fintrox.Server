namespace Fintrox.Contracts.Purchases;

public sealed record PurchaseDocumentLineResponse(
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
    decimal RecoverableVatPercent,
    decimal NetAmount,
    decimal VatAmount,
    decimal RecoverableVatAmount,
    decimal NonRecoverableVatAmount,
    decimal GrossAmount);
