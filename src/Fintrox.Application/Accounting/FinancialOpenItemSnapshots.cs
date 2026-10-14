namespace Fintrox.Application.Accounting;

/// <summary>One persisted financial document, including its current lifecycle state.</summary>
public sealed record FinancialOpenDocumentSnapshot(
    Guid Id,
    string SourceType,
    Guid CounterpartyId,
    string? Number,
    string Status,
    DateOnly DocumentDate,
    DateOnly DueDate,
    decimal GrossAmount,
    decimal? ExchangeRate);

/// <summary>Existing allocation and its payment lifecycle, not a derived payment.</summary>
public sealed record FinancialOpenAllocationSnapshot(
    Guid PaymentId,
    Guid PaymentCounterpartyId,
    Guid? SalesInvoiceId,
    Guid? PurchaseDocumentId,
    string PaymentStatus,
    string Direction,
    DateOnly PaymentDate,
    decimal DocumentAmount,
    decimal PaymentAmount,
    decimal PaymentAllocatedAmount,
    decimal PaymentGrossAmount);

/// <summary>One original document journal's control account (historical posting evidence).</summary>
public sealed record FinancialDocumentControlSnapshot(
    Guid DocumentId,
    string SourceType,
    Guid JournalEntryId,
    string JournalStatus,
    Guid AccountId,
    decimal Debit,
    decimal Credit);

/// <summary>Accumulated posted ledger balance on one tenant control account.</summary>
public sealed record FinancialControlLedgerSnapshot(
    Guid AccountId,
    decimal Debit,
    decimal Credit);

public sealed record FinancialPaymentHeaderSnapshot(
    Guid Id,
    string Status,
    string Direction,
    decimal AllocatedAmount,
    decimal GrossAmount);
