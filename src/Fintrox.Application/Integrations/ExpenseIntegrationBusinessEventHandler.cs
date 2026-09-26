using Fintrox.Application.Accounting;
using Fintrox.Application.Purchases;
using Fintrox.Contracts.Purchases;
using Fintrox.Domain.Purchases;

namespace Fintrox.Application.Integrations;

public sealed class ExpenseIntegrationBusinessEventProcessor(
    IPurchaseDocumentService purchaseDocuments,
    IJournalRepository journals) : IIntegrationBusinessEventProcessor
{
    public string Operation => IntegrationBusinessOperations.Expense;

    public async Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var payload = IntegrationPayloadSerializer
            .Deserialize<IntegrationExpensePayload>(payloadJson);

        var created = await purchaseDocuments.CreateAsync(
            new CreatePurchaseDocumentRequest(
                PurchaseDocumentType.Expense.ToString(),
                payload.CounterpartyId,
                payload.SupplierDocumentNumber,
                payload.DocumentDate,
                payload.DueDate,
                payload.CurrencyId,
                payload.Notes,
                payload.Lines),
            cancellationToken);

        var received = await purchaseDocuments.ReceiveAsync(
            created.Id,
            cancellationToken)
            ?? throw new IntegrationProcessingException(
                "The generated expense document could not be received.");

        var journal = await journals.GetSystemEntryByExternalReferenceAsync(
            received.OrganizationId,
            $"purchase-document:{received.Id:N}",
            trackChanges: false,
            cancellationToken);

        return new IntegrationBusinessEventResult(
            "PurchaseDocument",
            received.Id,
            journal?.Id,
            "purchase.expense.received",
            IntegrationPayloadSerializer.Serialize(received));
    }
}
