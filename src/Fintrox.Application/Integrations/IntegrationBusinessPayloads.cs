using Fintrox.Contracts.Counterparties;
using Fintrox.Contracts.Payments;
using Fintrox.Contracts.Purchases;
using Fintrox.Contracts.Sales;

namespace Fintrox.Application.Integrations;

public sealed record IntegrationSalesPayload(
    Guid CounterpartyId,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    Guid CurrencyId,
    string? Notes,
    IReadOnlyList<CreateSalesInvoiceLineRequest>? Lines);

public sealed record IntegrationPaymentPayload(
    string Direction,
    Guid CounterpartyId,
    DateOnly PaymentDate,
    string Method,
    Guid CurrencyId,
    decimal Amount,
    string? Reference,
    string? Notes,
    IReadOnlyList<CreatePaymentAllocationRequest>? Allocations);

public sealed record IntegrationExpensePayload(
    Guid CounterpartyId,
    string? SupplierDocumentNumber,
    DateOnly DocumentDate,
    DateOnly? DueDate,
    Guid CurrencyId,
    string? Notes,
    IReadOnlyList<CreatePurchaseDocumentLineRequest>? Lines);

public sealed record IntegrationCounterpartyPayload(
    CreateCounterpartyRequest Counterparty);
