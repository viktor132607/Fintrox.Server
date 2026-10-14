namespace Fintrox.Application.Accounting;

/// <summary>Current source-document state with original accounting event date.</summary>
public sealed record FinancialSourcePostingSnapshot(
    Guid Id, string SourceType, string Status, DateOnly DocumentDate,
    decimal GrossAmount, decimal? ExchangeRate);
