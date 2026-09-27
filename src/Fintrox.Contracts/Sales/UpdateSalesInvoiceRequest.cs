using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Sales;

public sealed record UpdateSalesInvoiceRequest(
    Guid CounterpartyId,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    Guid CurrencyId,
    [property: MaxLength(1000)] string? Notes);
