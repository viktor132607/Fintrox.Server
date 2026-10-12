namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollEarningType; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollEarningType : OrganizationEntity
{
    public PayrollEarningType(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string description,
        string calculationBasis,
        bool isTaxable)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        CalculationBasis = DomainRules.Text(calculationBasis, nameof(calculationBasis));
        IsTaxable = isTaxable;
    }

    public string Code { get; }
    public string Description { get; }
    public string CalculationBasis { get; }
    public bool IsTaxable { get; }
}
