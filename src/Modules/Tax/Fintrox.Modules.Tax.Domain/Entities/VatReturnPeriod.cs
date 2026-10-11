namespace Fintrox.Modules.Tax.Domain.Entities;

public sealed class VatReturnPeriod : OrganizationEntity
{
    public VatReturnPeriod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        DateOnly startsOn,
        DateOnly endsOn,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
        if (DueOn < EndsOn) throw new ArgumentException("Due date cannot precede the end of the period.", nameof(dueOn));
    }

    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public DateOnly DueOn { get; }
}
