namespace Fintrox.Application.Payments;

public sealed class PaymentConflictException(string message)
    : Exception(message);
