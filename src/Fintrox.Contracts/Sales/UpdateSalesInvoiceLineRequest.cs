using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Sales;

public sealed record UpdateSalesInvoiceLineRequest(
    [property: MaxLength(100)] string? ItemCode,
    [property: Required, MaxLength(500)] string Description,
    [property: Range(typeof(decimal), "0.000001", "999999999999.999999")] decimal Quantity,
    [property: MaxLength(32)] string? UnitOfMeasure,
    [property: Range(typeof(decimal), "0", "999999999999999.9999")] decimal UnitPrice,
    [property: Range(typeof(decimal), "0", "100")] decimal DiscountPercent,
    Guid VatCodeId);
