namespace Fintrox.Application.Currencies;

public sealed class CurrencyConflictException(string message)
    : Exception(message);
