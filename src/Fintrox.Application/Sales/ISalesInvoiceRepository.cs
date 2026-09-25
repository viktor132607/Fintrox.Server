using Fintrox.Domain.Sales;

namespace Fintrox.Application.Sales;

public interface ISalesInvoiceRepository
{
    Task<IReadOnlyList<SalesInvoice>> ListInvoicesAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        SalesInvoiceStatus? status,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<SalesInvoice?> GetInvoiceAsync(
        Guid organizationId,
        Guid invoiceId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SalesInvoiceLine>> ListLinesAsync(
        Guid organizationId,
        Guid invoiceId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<SalesInvoiceLine?> GetLineAsync(
        Guid organizationId,
        Guid invoiceId,
        Guid lineId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid invoiceId,
        CancellationToken cancellationToken);

    Task<long> AllocateInvoiceSequenceAsync(
        Guid organizationId,
        int calendarYear,
        CancellationToken cancellationToken);

    Task AddInvoiceAsync(
        SalesInvoice invoice,
        CancellationToken cancellationToken);

    Task AddLineAsync(
        SalesInvoiceLine line,
        CancellationToken cancellationToken);

    void RemoveLine(SalesInvoiceLine line);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
