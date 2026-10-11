namespace Fintrox.Modules.Payroll.Domain.Entities;

public sealed class PayrollPeriod : OrganizationEntity
{
    public PayrollPeriod(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        DateOnly startsOn,
        DateOnly endsOn,
        DateOnly payDate)
        : base(id, organizationId, createdAtUtc)
    {
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        PayDate = DomainRules.Date(payDate, nameof(payDate));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public DateOnly PayDate { get; }
}
