namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped PeriodCloseChecklist domain record; referenced objects are validated by application use cases.</summary>
public sealed class PeriodCloseChecklist : OrganizationEntity
{
    public PeriodCloseChecklist(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fiscalYearId,
        int periodNumber,
        string name,
        int requiredTaskCount)
        : base(id, organizationId, createdAtUtc)
    {
        FiscalYearId = DomainRules.Id(fiscalYearId, nameof(fiscalYearId));
        PeriodNumber = DomainRules.PositiveInt(periodNumber, nameof(periodNumber));
        Name = DomainRules.Text(name, nameof(name));
        RequiredTaskCount = DomainRules.NonnegativeInt(requiredTaskCount, nameof(requiredTaskCount));
    }

    public Guid FiscalYearId { get; }
    public int PeriodNumber { get; }
    public string Name { get; }
    public int RequiredTaskCount { get; }
}
