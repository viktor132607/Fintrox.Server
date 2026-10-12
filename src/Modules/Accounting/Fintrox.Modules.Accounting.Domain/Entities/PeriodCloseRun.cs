namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped PeriodCloseRun domain record; referenced objects are validated by application use cases.</summary>
public sealed class PeriodCloseRun : OrganizationEntity
{
    public PeriodCloseRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fiscalPeriodId,
        Guid startedByUserId,
        DateTimeOffset startedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalPeriodId = DomainRules.Id(fiscalPeriodId, nameof(fiscalPeriodId));
        StartedByUserId = DomainRules.Id(startedByUserId, nameof(startedByUserId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid FiscalPeriodId { get; }
    public Guid StartedByUserId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public string Status { get; }
}
