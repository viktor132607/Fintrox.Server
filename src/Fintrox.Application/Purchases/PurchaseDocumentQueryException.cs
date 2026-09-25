namespace Fintrox.Application.Purchases;

public sealed class PurchaseDocumentQueryException(string message)
    : Exception(message);
