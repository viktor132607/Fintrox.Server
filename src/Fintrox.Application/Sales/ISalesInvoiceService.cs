using Fintrox.Contracts.Sales;

namespace Fintrox.Application.Sales;

public interface ISalesInvoiceService
{
    Task<IReadOnlyList<SalesInvoiceSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<SalesInvoiceResponse?> GetAsync(
        Guid invoiceId,
        CancellationToken cancellationToken);

    Task<SalesInvoiceResponse> CreateAsync(
        CreateSalesInvoiceRequest request,
        CancellationToken cancellationToken);

    Task<SalesInvoiceResponse?> UpdateAsync(
        Guid invoiceId,
        UpdateSalesInvoiceRequest request,
        CancellationToken cancellationToken);

    Task<SalesInvoiceLineResponse?> AddLineAsync(
        Guid invoiceId,
        CreateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken);

    Task<SalesInvoiceLineResponse?> UpdateLineAsync(
        Guid invoiceId,
        Guid lineId,
        UpdateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteLineAsync(
        Guid invoiceId,
        Guid lineId,
        CancellationToken cancellationToken);

    Task<SalesInvoiceResponse?> IssueAsync(
        Guid invoiceId,
        CancellationToken cancellationToken);

    Task<SalesInvoiceResponse?> CancelAsync(
        Guid invoiceId,
        CancelSalesInvoiceRequest request,
        CancellationToken cancellationToken);
}
