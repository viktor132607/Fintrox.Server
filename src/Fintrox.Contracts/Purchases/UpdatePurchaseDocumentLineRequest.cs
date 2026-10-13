using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Purchases;

public sealed record UpdatePurchaseDocumentLineRequest(
    [param: MaxLength(100)] string? ItemCode,
    [param: Required, MaxLength(500)] string Description,
    [param: Range(typeof(decimal), "0.000001", "999999999999.999999")] decimal Quantity,
    [param: MaxLength(32)] string? UnitOfMeasure,
    [param: Range(typeof(decimal), "0", "999999999999999.9999")] decimal UnitPrice,
    [param: Range(typeof(decimal), "0", "100")] decimal DiscountPercent,
    Guid VatCodeId,
    [param: Range(typeof(decimal), "0", "100")] decimal RecoverableVatPercent = 100m);
