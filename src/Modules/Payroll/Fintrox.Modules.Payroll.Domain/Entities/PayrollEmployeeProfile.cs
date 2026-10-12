namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollEmployeeProfile; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollEmployeeProfile : OrganizationEntity
{
    public PayrollEmployeeProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string payrollNumber,
        string baseCurrency,
        string payFrequency,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        PayrollNumber = DomainRules.Text(payrollNumber, nameof(payrollNumber));
        BaseCurrency = DomainRules.Currency(baseCurrency, nameof(baseCurrency));
        PayFrequency = DomainRules.Text(payFrequency, nameof(payFrequency));
        IsActive = isActive;
    }

    public Guid EmployeeId { get; }
    public string PayrollNumber { get; }
    public string BaseCurrency { get; }
    public string PayFrequency { get; }
    public bool IsActive { get; }
}
