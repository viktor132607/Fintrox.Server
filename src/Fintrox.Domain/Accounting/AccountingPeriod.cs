using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class AccountingPeriod : OrganizationScopedAuditableEntity
{
    private AccountingPeriod()
    {
    }

    private AccountingPeriod(
        Guid id,
        Guid organizationId,
        Guid fiscalYearId,
        int number,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (fiscalYearId == Guid.Empty)
        {
            throw new ArgumentException(
                "Fiscal year id is required.",
                nameof(fiscalYearId));
        }

        ValidateNumber(number);
        ValidateRange(startDate, endDate);

        FiscalYearId = fiscalYearId;
        Number = number;
        Name = NormalizeName(name);
        StartDate = startDate;
        EndDate = endDate;
        Status = AccountingPeriodStatus.Open;
    }

    public Guid FiscalYearId { get; private set; }

    public int Number { get; private set; }

    public string Name { get; private set; } = null!;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public AccountingPeriodStatus Status { get; private set; }

    public static AccountingPeriod Create(
        Guid organizationId,
        Guid fiscalYearId,
        int number,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now)
    {
        return new AccountingPeriod(
            Guid.NewGuid(),
            organizationId,
            fiscalYearId,
            number,
            name,
            startDate,
            endDate,
            now);
    }

    public void Update(
        int number,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now)
    {
        EnsureOpen();
        ValidateNumber(number);
        ValidateRange(startDate, endDate);

        Number = number;
        Name = NormalizeName(name);
        StartDate = startDate;
        EndDate = endDate;
        Touch(now);
    }

    public void SoftClose(DateTimeOffset now)
    {
        if (Status == AccountingPeriodStatus.SoftClosed)
        {
            return;
        }

        if (Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed accounting period must be reopened before it can be soft-closed.");
        }

        Status = AccountingPeriodStatus.SoftClosed;
        Touch(now);
    }

    public void Close(DateTimeOffset now)
    {
        if (Status == AccountingPeriodStatus.Closed)
        {
            return;
        }

        Status = AccountingPeriodStatus.Closed;
        Touch(now);
    }

    public void Reopen(DateTimeOffset now)
    {
        if (Status == AccountingPeriodStatus.Open)
        {
            return;
        }

        Status = AccountingPeriodStatus.Open;
        Touch(now);
    }

    public bool Contains(DateOnly date) =>
        date >= StartDate && date <= EndDate;

    private void EnsureOpen()
    {
        if (Status != AccountingPeriodStatus.Open)
        {
            throw new InvalidOperationException(
                "Only an open accounting period can be modified.");
        }
    }

    private static void ValidateNumber(int number)
    {
        if (number is < 1 or > 99)
        {
            throw new ArgumentOutOfRangeException(
                nameof(number),
                "Accounting period number must be between 1 and 99.");
        }
    }

    private static void ValidateRange(
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "Accounting period end date cannot be before its start date.");
        }
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Accounting period name is required.",
                nameof(value));
        }

        if (normalized.Length > 80)
        {
            throw new ArgumentException(
                "Accounting period name cannot exceed 80 characters.",
                nameof(value));
        }

        return normalized;
    }
}
