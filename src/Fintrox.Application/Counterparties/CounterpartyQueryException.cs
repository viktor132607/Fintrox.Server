namespace Fintrox.Application.Counterparties;

public sealed class CounterpartyQueryException(string message)
    : Exception(message);
