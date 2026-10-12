namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollBenefitType; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollBenefitType : OrganizationEntity
{
    public PayrollBenefitType(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string taxTreatmentCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        TaxTreatmentCode = DomainRules.Text(taxTreatmentCode, nameof(taxTreatmentCode));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string TaxTreatmentCode { get; }
    public bool IsActive { get; }
}
