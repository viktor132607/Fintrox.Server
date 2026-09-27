using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CancelPaymentRequest(
    [property: Required, MaxLength(500)] string Reason);
