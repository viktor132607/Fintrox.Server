using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Sales;

public sealed record CancelSalesInvoiceRequest(
    [property: Required, MaxLength(500)] string Reason);
