namespace Fintrox.Application.Counterparties;

public sealed class CounterpartyConflictException(string message)
    : Exception(message);
