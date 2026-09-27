using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Purchases;

public sealed record CreatePurchaseDocumentRequest(
    [property: Required] string Type,
    Guid CounterpartyId,
    [property: MaxLength(80)] string? SupplierDocumentNumber,
    DateOnly DocumentDate,
    DateOnly? DueDate,
    Guid CurrencyId,
    [property: MaxLength(1000)] string? Notes,
    IReadOnlyList<CreatePurchaseDocumentLineRequest>? Lines);
