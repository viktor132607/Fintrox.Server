namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollTaxWithholding; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollTaxWithholding : OrganizationEntity
{
    public PayrollTaxWithholding(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payslipId,
        string taxCode,
        decimal taxableBase,
        decimal withheldAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayslipId = DomainRules.Id(payslipId, nameof(payslipId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        TaxableBase = DomainRules.Nonnegative(taxableBase, nameof(taxableBase));
        WithheldAmount = DomainRules.Nonnegative(withheldAmount, nameof(withheldAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PayslipId { get; }
    public string TaxCode { get; }
    public decimal TaxableBase { get; }
    public decimal WithheldAmount { get; }
    public string Currency { get; }
}
