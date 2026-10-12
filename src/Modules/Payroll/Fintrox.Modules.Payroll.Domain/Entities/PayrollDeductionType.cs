namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollDeductionType; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollDeductionType : OrganizationEntity
{
    public PayrollDeductionType(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string description,
        string deductionCategory,
        bool isMandatory)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        DeductionCategory = DomainRules.Text(deductionCategory, nameof(deductionCategory));
        IsMandatory = isMandatory;
    }

    public string Code { get; }
    public string Description { get; }
    public string DeductionCategory { get; }
    public bool IsMandatory { get; }
}
