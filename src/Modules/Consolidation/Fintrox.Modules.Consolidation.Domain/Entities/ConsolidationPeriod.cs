namespace Fintrox.Modules.Consolidation.Domain.Entities;

public sealed class ConsolidationPeriod : OrganizationEntity
{
    public ConsolidationPeriod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid CompanyGroupId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
