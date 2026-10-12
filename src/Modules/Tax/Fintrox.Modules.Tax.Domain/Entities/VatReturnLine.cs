namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatReturnLine; tenant scope and input fields validated at creation.</summary>
public sealed class VatReturnLine : OrganizationEntity
{
    public VatReturnLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid vatReturnId,
        string boxCode,
        decimal taxableAmount,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        VatReturnId = DomainRules.Id(vatReturnId, nameof(vatReturnId));
        BoxCode = DomainRules.Text(boxCode, nameof(boxCode));
        TaxableAmount = DomainRules.Nonnegative(taxableAmount, nameof(taxableAmount));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid VatReturnId { get; }
    public string BoxCode { get; }
    public decimal TaxableAmount { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
