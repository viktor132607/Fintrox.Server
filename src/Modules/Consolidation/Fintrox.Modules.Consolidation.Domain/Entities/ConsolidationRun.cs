namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationRun; external references validated by application layer.</summary>
public sealed class ConsolidationRun : OrganizationEntity
{
    public ConsolidationRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid consolidationPeriodId,
        int runNumber,
        DateTimeOffset startedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        ConsolidationPeriodId = DomainRules.Id(consolidationPeriodId, nameof(consolidationPeriodId));
        RunNumber = DomainRules.PositiveInt(runNumber, nameof(runNumber));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CompanyGroupId { get; }
    public Guid ConsolidationPeriodId { get; }
    public int RunNumber { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public string Status { get; }
}
