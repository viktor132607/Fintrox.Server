namespace Fintrox.Application.Sales;

public sealed class SalesInvoiceQueryException(string message)
    : Exception(message);
