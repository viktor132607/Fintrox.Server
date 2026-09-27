using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class JournalLine : OrganizationScopedAuditableEntity
{
    private JournalLine()
    {
    }

    private JournalLine(
        Guid id,
        Guid organizationId,
        Guid journalEntryId,
        int lineNumber,
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (journalEntryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Journal entry id is required.",
                nameof(journalEntryId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "Account id is required.",
                nameof(accountId));
        }

        ValidateLineNumber(lineNumber);
        ValidateAmounts(debit, credit);

        JournalEntryId = journalEntryId;
        LineNumber = lineNumber;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Description = NormalizeDescription(description);
    }

    public Guid JournalEntryId { get; private set; }

    public int LineNumber { get; private set; }

    public Guid AccountId { get; private set; }

    public decimal Debit { get; private set; }

    public decimal Credit { get; private set; }

    public string? Description { get; private set; }

    public static JournalLine Create(
        Guid organizationId,
        Guid journalEntryId,
        int lineNumber,
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description,
        DateTimeOffset now)
    {
        return new JournalLine(
            Guid.NewGuid(),
            organizationId,
            journalEntryId,
            lineNumber,
            accountId,
            debit,
            credit,
            description,
            now);
    }

    public void Update(
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description,
        DateTimeOffset now)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "Account id is required.",
                nameof(accountId));
        }

        ValidateAmounts(debit, credit);

        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Description = NormalizeDescription(description);
        Touch(now);
    }

    private static void ValidateLineNumber(int lineNumber)
    {
        if (lineNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lineNumber),
                "Journal line number must be positive.");
        }
    }

    private static void ValidateAmounts(decimal debit, decimal credit)
    {
        if (debit < 0m || credit < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(debit),
                "Debit and credit amounts cannot be negative.");
        }

        if ((debit == 0m && credit == 0m) ||
            (debit > 0m && credit > 0m))
        {
            throw new ArgumentException(
                "A journal line must contain a positive amount on exactly one side.");
        }

        if (decimal.Round(debit, 4) != debit ||
            decimal.Round(credit, 4) != credit)
        {
            throw new ArgumentException(
                "Journal amounts support at most four decimal places.");
        }
    }

    private static string? NormalizeDescription(string? value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > 500)
        {
            throw new ArgumentException(
                "Journal line description cannot exceed 500 characters.",
                nameof(value));
        }

        return normalized;
    }
}
