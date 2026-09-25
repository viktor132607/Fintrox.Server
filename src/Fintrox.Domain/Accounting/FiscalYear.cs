using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class FiscalYear : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private FiscalYear()
    {
    }

    private FiscalYear(
        Guid id,
        Guid organizationId,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Name = NormalizeName(name);
        ValidateRange(startDate, endDate);
        StartDate = startDate;
        EndDate = endDate;
        Status = FiscalYearStatus.Open;
    }

    public string Name { get; private set; } = null!;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public FiscalYearStatus Status { get; private set; }

    public static FiscalYear Create(
        Guid organizationId,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now)
    {
        return new FiscalYear(
            Guid.NewGuid(),
            organizationId,
            name,
            startDate,
            endDate,
            now);
    }

    public void Update(
        string name,
        DateOnly startDate,
        DateOnly endDate,
        DateTimeOffset now)
    {
        EnsureOpen();
        ValidateRange(startDate, endDate);

        Name = NormalizeName(name);
        StartDate = startDate;
        EndDate = endDate;
        Touch(now);
    }

    public void SoftClose(DateTimeOffset now)
    {
        if (Status == FiscalYearStatus.SoftClosed)
        {
            return;
        }

        if (Status == FiscalYearStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed fiscal year must be reopened before it can be soft-closed.");
        }

        Status = FiscalYearStatus.SoftClosed;
        Touch(now);
    }

    public void Close(DateTimeOffset now)
    {
        if (Status == FiscalYearStatus.Closed)
        {
            return;
        }

        Status = FiscalYearStatus.Closed;
        Touch(now);
    }

    public void Reopen(DateTimeOffset now)
    {
        if (Status == FiscalYearStatus.Open)
        {
            return;
        }

        Status = FiscalYearStatus.Open;
        Touch(now);
    }

    public bool Contains(DateOnly date) =>
        date >= StartDate && date <= EndDate;

    public bool Contains(DateOnly startDate, DateOnly endDate) =>
        startDate >= StartDate && endDate <= EndDate;

    private void EnsureOpen()
    {
        if (Status != FiscalYearStatus.Open)
        {
            throw new InvalidOperationException(
                "Only an open fiscal year can be modified.");
        }
    }

    private static void ValidateRange(
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "Fiscal year end date cannot be before its start date.");
        }
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Fiscal year name is required.",
                nameof(value));
        }

        if (normalized.Length > 80)
        {
            throw new ArgumentException(
                "Fiscal year name cannot exceed 80 characters.",
                nameof(value));
        }

        return normalized;
    }
}
