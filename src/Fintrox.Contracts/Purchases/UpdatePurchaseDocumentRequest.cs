using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Purchases;

public sealed record UpdatePurchaseDocumentRequest(
    [param: Required] string Type,
    Guid CounterpartyId,
    [param: MaxLength(80)] string? SupplierDocumentNumber,
    DateOnly DocumentDate,
    DateOnly? DueDate,
    Guid CurrencyId,
    [param: MaxLength(1000)] string? Notes);
