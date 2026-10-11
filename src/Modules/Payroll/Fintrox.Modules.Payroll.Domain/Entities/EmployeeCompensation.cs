namespace Fintrox.Modules.Payroll.Domain.Entities;

public sealed class EmployeeCompensation : OrganizationEntity
{
    public EmployeeCompensation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateOnly effectiveFrom,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid EmployeeId { get; }
    public DateOnly EffectiveFrom { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
