using Fintrox.Application.Accounting;
using Fintrox.Application.Sales;
using Fintrox.Contracts.Sales;

namespace Fintrox.Application.Integrations;

public sealed class SalesIntegrationBusinessEventHandler(
    ISalesInvoiceService salesInvoices,
    IJournalRepository journals) : IIntegrationBusinessEventHandler
{
    public string Operation => IntegrationBusinessOperations.Sales;

    public async Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var payload = IntegrationPayloadSerializer
            .Deserialize<IntegrationSalesPayload>(payloadJson);

        var created = await salesInvoices.CreateAsync(
            new CreateSalesInvoiceRequest(
                payload.CounterpartyId,
                payload.InvoiceDate,
                payload.DueDate,
                payload.CurrencyId,
                payload.Notes,
                payload.Lines),
            cancellationToken);

        var issued = await salesInvoices.IssueAsync(
            created.Id,
            cancellationToken)
            ?? throw new IntegrationProcessingException(
                "The generated sales invoice could not be issued.");

        var journal = await journals.GetSystemEntryByExternalReferenceAsync(
            issued.OrganizationId,
            $"sales-invoice:{issued.Id:N}",
            trackChanges: false,
            cancellationToken);

        return new IntegrationBusinessEventResult(
            "SalesInvoice",
            issued.Id,
            journal?.Id,
            "sales.invoice.issued",
            IntegrationPayloadSerializer.Serialize(issued));
    }
}
