using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CreatePaymentRequest(
    [property: Required] string Direction,
    Guid CounterpartyId,
    DateOnly PaymentDate,
    [property: Required] string Method,
    Guid CurrencyId,
    [property: Range(typeof(decimal), "0.0001", "999999999999999.9999")] decimal Amount,
    [property: MaxLength(120)] string? Reference,
    [property: MaxLength(1000)] string? Notes);
