using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CancelPaymentRequest(
    [param: Required, MaxLength(500)] string Reason);
