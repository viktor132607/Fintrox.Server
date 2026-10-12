namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetDepreciationRun domain model.</summary>
public sealed class AssetDepreciationRun : OrganizationEntity
{
    public AssetDepreciationRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingBookId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        DateTimeOffset runAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        RunAtUtc = DomainRules.Instant(runAtUtc, nameof(runAtUtc));
        Status = DomainRules.Text(status, nameof(status));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("End must not precede start.", nameof(periodEndsOn));
    }

    public Guid AccountingBookId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public DateTimeOffset RunAtUtc { get; }
    public string Status { get; }
}
