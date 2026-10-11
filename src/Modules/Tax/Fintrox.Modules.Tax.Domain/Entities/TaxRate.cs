namespace Fintrox.Modules.Tax.Domain.Entities;

public sealed class TaxRate : OrganizationEntity
{
    public TaxRate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jurisdictionId,
        Guid categoryId,
        decimal ratePercent,
        DateOnly effectiveFrom,
        DateOnly effectiveTo)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        CategoryId = DomainRules.Id(categoryId, nameof(categoryId));
        RatePercent = DomainRules.Percent(ratePercent, nameof(ratePercent));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        EffectiveTo = DomainRules.Date(effectiveTo, nameof(effectiveTo));
        if (EffectiveTo < EffectiveFrom) throw new ArgumentException("The end cannot precede the start.", nameof(effectiveTo));
    }

    public Guid JurisdictionId { get; }
    public Guid CategoryId { get; }
    public decimal RatePercent { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly EffectiveTo { get; }
}
