namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class MarketingCampaign : OrganizationEntity
{
    public MarketingCampaign(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public string Name { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
