using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CreatePaymentRequest(
    [param: Required] string Direction,
    Guid CounterpartyId,
    DateOnly PaymentDate,
    [param: Required] string Method,
    Guid CurrencyId,
    [param: Range(typeof(decimal), "0.0001", "999999999999999.9999")] decimal Amount,
    [param: MaxLength(120)] string? Reference,
    [param: MaxLength(1000)] string? Notes);
