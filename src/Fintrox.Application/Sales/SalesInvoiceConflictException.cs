namespace Fintrox.Application.Sales;

public sealed class SalesInvoiceConflictException(string message)
    : Exception(message);
