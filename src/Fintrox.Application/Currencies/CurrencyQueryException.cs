namespace Fintrox.Application.Currencies;

public sealed class CurrencyQueryException(string message)
    : Exception(message);
