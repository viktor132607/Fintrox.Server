namespace Fintrox.Application.Purchases;

public sealed class PurchaseDocumentConflictException(string message)
    : Exception(message);
