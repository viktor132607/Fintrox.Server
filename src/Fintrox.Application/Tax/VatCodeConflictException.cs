namespace Fintrox.Application.Tax;

public sealed class VatCodeConflictException(string message)
    : Exception(message);
