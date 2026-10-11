namespace Fintrox.Modules.Commerce.Domain.Entities;

public sealed class Promotion : OrganizationEntity
{
    public Promotion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal discountPercent)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        DiscountPercent = DomainRules.Percent(discountPercent, nameof(discountPercent));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public string Code { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal DiscountPercent { get; }
}
