using Fintrox.Application.Payments;
using Fintrox.Domain.Payments;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Payments;

public sealed class PaymentRepository(
    FintroxDbContext dbContext) : IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> ListPaymentsAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        PaymentStatus? status,
        PaymentDirection? direction,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        IQueryable<Payment> query = dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrganizationId == organizationId);

        if (fromDate.HasValue)
        {
            query = query.Where(payment =>
                payment.PaymentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(payment =>
                payment.PaymentDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(payment =>
                payment.Status == status.Value);
        }

        if (direction.HasValue)
        {
            query = query.Where(payment =>
                payment.Direction == direction.Value);
        }

        if (counterpartyId.HasValue)
        {
            query = query.Where(payment =>
                payment.CounterpartyId == counterpartyId.Value);
        }

        return await query
            .OrderByDescending(payment => payment.PaymentDate)
            .ThenByDescending(payment => payment.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Payment?> GetPaymentAsync(
        Guid organizationId,
        Guid paymentId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Payment> query = dbContext.Payments;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            payment =>
                payment.OrganizationId == organizationId &&
                payment.Id == paymentId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentAllocation>> ListAllocationsAsync(
        Guid organizationId,
        Guid paymentId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<PaymentAllocation> query = dbContext.PaymentAllocations
            .Where(allocation =>
                allocation.OrganizationId == organizationId &&
                allocation.PaymentId == paymentId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(allocation => allocation.LineNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PaymentAllocation?> GetAllocationAsync(
        Guid organizationId,
        Guid paymentId,
        Guid allocationId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<PaymentAllocation> query = dbContext.PaymentAllocations;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            allocation =>
                allocation.OrganizationId == organizationId &&
                allocation.PaymentId == paymentId &&
                allocation.Id == allocationId,
            cancellationToken);
    }

    public async Task<int> GetNextAllocationLineNumberAsync(
        Guid organizationId,
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var max = await dbContext.PaymentAllocations
            .Where(allocation =>
                allocation.OrganizationId == organizationId &&
                allocation.PaymentId == paymentId)
            .Select(allocation => (int?)allocation.LineNumber)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }

    public async Task<decimal> GetConfirmedSalesInvoiceAllocatedAmountAsync(
        Guid organizationId,
        Guid salesInvoiceId,
        Guid? excludingPaymentId,
        CancellationToken cancellationToken)
    {
        var amount = await (
                from allocation in dbContext.PaymentAllocations.AsNoTracking()
                join payment in dbContext.Payments.AsNoTracking()
                    on new
                    {
                        allocation.PaymentId,
                        allocation.OrganizationId
                    }
                    equals new
                    {
                        PaymentId = payment.Id,
                        payment.OrganizationId
                    }
                where
                    allocation.OrganizationId == organizationId &&
                    allocation.SalesInvoiceId == salesInvoiceId &&
                    payment.Status == PaymentStatus.Confirmed &&
                    (!excludingPaymentId.HasValue ||
                     payment.Id != excludingPaymentId.Value)
                select (decimal?)allocation.DocumentAmount)
            .SumAsync(cancellationToken);

        return amount ?? 0m;
    }

    public async Task<decimal> GetConfirmedPurchaseDocumentAllocatedAmountAsync(
        Guid organizationId,
        Guid purchaseDocumentId,
        Guid? excludingPaymentId,
        CancellationToken cancellationToken)
    {
        var amount = await (
                from allocation in dbContext.PaymentAllocations.AsNoTracking()
                join payment in dbContext.Payments.AsNoTracking()
                    on new
                    {
                        allocation.PaymentId,
                        allocation.OrganizationId
                    }
                    equals new
                    {
                        PaymentId = payment.Id,
                        payment.OrganizationId
                    }
                where
                    allocation.OrganizationId == organizationId &&
                    allocation.PurchaseDocumentId == purchaseDocumentId &&
                    payment.Status == PaymentStatus.Confirmed &&
                    (!excludingPaymentId.HasValue ||
                     payment.Id != excludingPaymentId.Value)
                select (decimal?)allocation.DocumentAmount)
            .SumAsync(cancellationToken);

        return amount ?? 0m;
    }

    public async Task LockSalesInvoiceAsync(
        Guid organizationId,
        Guid salesInvoiceId,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.SalesInvoices
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM sales.invoices
                WHERE id = {salesInvoiceId}
                  AND organization_id = {organizationId}
                FOR UPDATE
                """)
            .AsNoTracking()
            .Select(invoice => invoice.Id)
            .SingleAsync(cancellationToken);
    }

    public async Task LockPurchaseDocumentAsync(
        Guid organizationId,
        Guid purchaseDocumentId,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.PurchaseDocuments
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM purchases.documents
                WHERE id = {purchaseDocumentId}
                  AND organization_id = {organizationId}
                FOR UPDATE
                """)
            .AsNoTracking()
            .Select(document => document.Id)
            .SingleAsync(cancellationToken);
    }

    public async Task<long> AllocatePaymentSequenceAsync(
        Guid organizationId,
        int calendarYear,
        PaymentDirection direction,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Payment numbers must be allocated inside a database transaction.");
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO payments.payment_number_sequences
                (organization_id, calendar_year, direction, last_number)
            VALUES
                ({organizationId}, {calendarYear}, {direction.ToString()}, 1)
            ON CONFLICT (organization_id, calendar_year, direction)
            DO UPDATE SET
                last_number = payments.payment_number_sequences.last_number + 1
            """,
            cancellationToken);

        return await dbContext.PaymentNumberSequences
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.CalendarYear == calendarYear &&
                sequence.Direction == direction)
            .Select(sequence => sequence.LastNumber)
            .SingleAsync(cancellationToken);
    }

    public async Task AddPaymentAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task AddAllocationAsync(
        PaymentAllocation allocation,
        CancellationToken cancellationToken)
    {
        await dbContext.PaymentAllocations.AddAsync(
            allocation,
            cancellationToken);
    }

    public void RemoveAllocation(PaymentAllocation allocation)
    {
        dbContext.PaymentAllocations.Remove(allocation);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
