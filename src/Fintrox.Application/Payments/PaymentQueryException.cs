namespace Fintrox.Application.Payments;

public sealed class PaymentQueryException(string message)
    : Exception(message);
